using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Configuration;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.VisualStudio.Shell.Interop;
using Refactorizer.VSIX.Exception;
using Refactorizer.VSIX.Models;
using MSDocument = Microsoft.CodeAnalysis.Document;
using MSSolution = Microsoft.CodeAnalysis.Solution;
using MSProject = Microsoft.CodeAnalysis.Project;
using Package = Microsoft.VisualStudio.Shell.Package;
using Project = Refactorizer.VSIX.Models.Project;
using Property = Refactorizer.VSIX.Models.Property;
using Solution = Refactorizer.VSIX.Models.Solution;

namespace Refactorizer.VSIX
{
    // TODO: Split in multiple classes
    internal class CodeAnalyser
    {
        private readonly ClassnameFormater _classnameFormater;

        private Dictionary<Guid, List<string>> _methodToMetod = new Dictionary<Guid, List<string>>();

        private Dictionary<Guid, List<string>> _classToClass = new Dictionary<Guid, List<string>>();

        private Dictionary<Guid, List<string>> _namespaceToNamespace = new Dictionary<Guid, List<string>>();

        private Dictionary<Guid, List<ProjectId>> _projectToProject = new Dictionary<Guid, List<ProjectId>>();

        private Dictionary<Guid, string> _propertyToType = new Dictionary<Guid, string>();

        private Dictionary<Guid, string> _fieldToType = new Dictionary<Guid, string>();

        public CodeAnalyser()
        {
            _classnameFormater = new ClassnameFormater();
        }

        private async Task<MSSolution> GetCurrentSolution()
        {
            var dte = Package.GetGlobalService(typeof(SDTE)) as DTE;
            var solutionFullName = dte?.Solution.FullName;
            if (string.IsNullOrEmpty(solutionFullName))
                throw new NoSolutionOpenException();

            var msBuildWorkspace = MSBuildWorkspace.Create();
            return await msBuildWorkspace.OpenSolutionAsync(solutionFullName);
        }

        /// <summary>
        ///     Generates a new dependency solution based on opened solution
        /// </summary>
        /// <returns></returns>
        public async Task<Solution> GenerateDependencyTree()
        {
            var solution = new Solution();

            // Reset temp storages
            _classToClass = new Dictionary<Guid, List<string>>();
            _namespaceToNamespace = new Dictionary<Guid, List<string>>();
            _projectToProject = new Dictionary<Guid, List<ProjectId>>();

            // TODO: Handle all parallel to improve performance
            foreach (var msProject in (await GetCurrentSolution()).Projects)
            {
                var referencedProjectIds = msProject.ProjectReferences.Select(x => x.ProjectId).ToList();
                var project = new Project(Guid.NewGuid(), msProject.Id, msProject.Name);
                _projectToProject.Add(project.Id, referencedProjectIds);
                await AddDocuments(msProject, project);

                solution.Projects.Add(project);
            }

            // TODO: Handle all parallel after code above to improve performance
            AddReferencesBetweenProjects(solution);
            AddReferencesBetweenNamespaces(solution);
            AddReferencesBetweenClasses(solution);

            return solution;
        }

        private void AddReferencesBetweenProjects(Solution solution)
        {
            foreach (var project in solution.Projects)
                if (_projectToProject.ContainsKey(project.Id) && _projectToProject[project.Id] != null)
                    foreach (var referencesId in _projectToProject[project.Id])
                    {
                        var reference = GetProjectById(solution, referencesId);
                        if (reference != null)
                            project.References.Add(reference);
                    }
        }

        private Project GetProjectById(Solution solution, ProjectId referencesId)
        {
            return solution.Projects.FirstOrDefault(project => project.ProjectId.Equals(referencesId));
        }

        private void AddReferencesBetweenNamespaces(Solution solution)
        {
            foreach (var project in solution.Projects)
            foreach (var @namespace in project.Namespaces)
                if (_namespaceToNamespace.ContainsKey(@namespace.Id) && _namespaceToNamespace[@namespace.Id] != null)
                    foreach (var namespaceReference in _namespaceToNamespace[@namespace.Id])
                    {
                        var reference = GetNamespaceByName(solution, namespaceReference);
                        if (reference != null)
                            @namespace.References.Add(reference);
                    }
        }

        private Namespace GetNamespaceByName(Solution solution, string namespaceName)
        {
            return solution.Projects.SelectMany(project => project.Namespaces)
                .FirstOrDefault(ns => ns.Name.Equals(namespaceName));
        }

        // Change name of class references to fullname
        // We can do this only after knowing all namespaces and classes at project
        private void AddReferencesBetweenClasses(Solution solution)
        {
            foreach (var project in solution.Projects)
            foreach (var @namespace in project.Namespaces)
            foreach (var @class in @namespace.Classes)
                if (_classToClass.ContainsKey(@class.Id) && _classToClass[@class.Id] != null)
                    foreach (var nameReference in _classToClass[@class.Id])
                    {
                        // TODO: does this realy select the correct reference if there is duplicate classname in different namespaces?
                        // We should only check referenced namespaced from that defined in the same file as the ClassDeclarationSyntax
                        // Closed enough for prototype
                        var reference = GetClassByNameFromNamespace(@namespace, nameReference);
                        if (reference == null)
                            foreach (var namespaceReference in @namespace.References)
                            {
                                reference = GetClassByNameFromNamespace((Namespace) namespaceReference, nameReference);
                                if (reference != null)
                                    break;
                            }
                        if (reference != null) @class.References.Add(reference);
                    }
        }

        private Class GetClassByNameFromNamespace(Namespace @namespace, string name)
        {
            return @namespace.Classes.FirstOrDefault(@class => @class.Name.Equals(name));
        }

        private async Task AddDocuments(MSProject msProject, Project project)
        {
            // Handle all documents in project
            foreach (var msDocument in msProject.Documents)
                await AddNamespaces(project, msDocument);
        }

        private async Task AddNamespaces(Project project, MSDocument msDocument)
        {
            // We use the syntax tree to find all declareded classes inside this dockument
            var documentSyntaxRoot = await msDocument.GetSyntaxRootAsync();

            // We need the semanic model to query some informations of nodes
            var model = await msDocument.GetSemanticModelAsync();

            var referencedNamespaces = GetRelatedNamespaces(documentSyntaxRoot, model);

            // Use the syntax tree to get all namepace definitions inside
            var namespaces = documentSyntaxRoot.DescendantNodesAndSelf().OfType<NamespaceDeclarationSyntax>();

            foreach (var namespaceDeclarationSyntax in namespaces)
            {
                // Get the syntrax tree based on namespace
                var namespaceSyntaxRoot = await namespaceDeclarationSyntax.SyntaxTree.GetRootAsync();

                // Get the symbol for namespace from model to get full name of namespace
                var namespaceSymbol = model.GetDeclaredSymbol(namespaceDeclarationSyntax);

                var namespaceName = namespaceSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);

                // Check for exisiting namespace
                var @namespace = project.Namespaces.FirstOrDefault(x => x.Name.Equals(namespaceName));
                if (@namespace == null)
                {
                    @namespace = new Namespace(Guid.NewGuid(), namespaceName, project);
                    project.Namespaces.Add(@namespace);
                    _namespaceToNamespace.Add(@namespace.Id, referencedNamespaces);
                }
                else
                {
                    _namespaceToNamespace[@namespace.Id] = _namespaceToNamespace[@namespace.Id]
                        .Union(referencedNamespaces).ToList();
                }

                // Use the syntax tree to get all classes declarations inside
                var classes = namespaceSyntaxRoot.DescendantNodes().OfType<ClassDeclarationSyntax>();

                // Handle each class declaration inside the namespace
                foreach (var classDeclaration in classes)
                {
                    var symbol = model.GetDeclaredSymbol(classDeclaration);
                    var baseList = classDeclaration.BaseList;
                    var root = await classDeclaration.SyntaxTree.GetRootAsync();
                    var referencedClasses = GetClassReferences(root, baseList);

                    CreateClass(root, symbol, namespaceName, @namespace, referencedClasses);
                }

                // Use the syntax tree to get all interface declations inside
                var interfaces = namespaceSyntaxRoot.DescendantNodes().OfType<InterfaceDeclarationSyntax>();

                foreach (var interfaceDeclaration in interfaces)
                {
                    var symbol = model.GetDeclaredSymbol(interfaceDeclaration);
                    var baseList = interfaceDeclaration.BaseList;
                    var root = await interfaceDeclaration.SyntaxTree.GetRootAsync();
                    var referencedClasses = GetClassReferences(root, baseList);

                    CreateClass(root, symbol, namespaceName, @namespace, referencedClasses);
                }
            }
        }
        private List<string> GetRelatedNamespaces(SyntaxNode syntaxTree, SemanticModel model)
        {
            var usings = syntaxTree.DescendantNodes().OfType<UsingDirectiveSyntax>();

            // Collection of related namespace, we use to find the correct reference
            var relatedNamespaces = new List<string>();

            foreach (var usingDirectiveSyntax in usings)
            {
                var info = model.GetSymbolInfo(usingDirectiveSyntax.Name);
                var symbol = info.Symbol;
                if (symbol == null)
                    continue;

                relatedNamespaces.Add(symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
            }

            return relatedNamespaces;
        }

        private void CreateClass(SyntaxNode root, ISymbol symbol, string namespaceName, Namespace @namespace, List<string> referencedClasses)
        {
            var className = symbol.Name;
            // Find all referenced classes inside this class

            var @class = new Class(Guid.NewGuid(),
                _classnameFormater.FormatClassFullName(className, namespaceName), className, @namespace);
            _classToClass.Add(@class.Id, referencedClasses);

            @namespace.Classes.Add(@class);

            AddFields(root, @class);
            AddProperties(root, @class);
            AddMethods(root, @class);
        }

        // TODO: Handle generics https://stackoverflow.com/questions/43210137/how-to-check-if-method-parameter-type-return-type-is-generic-in-roslyn
        private void AddMethods(SyntaxNode syntaxTree, Class @class)
        {
            var allReferences = new List<string>();
            
            // Constructors
            var constructorDeclarations = syntaxTree.DescendantNodes().OfType<ConstructorDeclarationSyntax>();
            foreach (var constructorDeclaration in constructorDeclarations)
            {
                var methodName = constructorDeclaration.Identifier.ToString();
                var parameterDeclarations = constructorDeclaration.ParameterList.Parameters;

                var references = new List<string>();
                var parameterAsString = GetMethodParametersAsStringList(parameterDeclarations, references);

                var method = new Method(Guid.NewGuid(), methodName, @class )
                {
                    Signature =  $"{methodName}({parameterAsString})"
                };

                @class.Methods.Add(method);
                _methodToMetod.Add(method.Id, references);
            }

            // Methods
            var methodDeclarations = syntaxTree.DescendantNodes().OfType<MethodDeclarationSyntax>();
            foreach (var methodDeclartion in methodDeclarations)
            {
                var methodName = methodDeclartion.Identifier.ToString();
                var returnType = methodDeclartion.ReturnType.ToString();
                allReferences.Add(returnType);
                var parameterDeclarations = methodDeclartion.ParameterList.Parameters;

                var references = new List<string>();
                var parameterAsString = GetMethodParametersAsStringList(parameterDeclarations, references);

                var method = new Method(Guid.NewGuid(), methodName, @class )
                {
                    ReturnType = returnType,
                    Signature =  $"{returnType} {methodName}({parameterAsString})"
                };

                @class.Methods.Add(method);
                _methodToMetod.Add(method.Id, references);
            }
            _classToClass[@class.Id] = _classToClass[@class.Id].Union(allReferences).ToList();
        }

        private string GetMethodParametersAsStringList(SeparatedSyntaxList<ParameterSyntax> parameterDeclarations, List<string> references)
        {
            var parametersAsListOfStrings = new List<string>();
            foreach (var parameter in parameterDeclarations)
            {
                var parameterName = parameter.Identifier.ToString();
                var parameterType = parameter.Type.ToString();
                references.Add(parameterType);

                parametersAsListOfStrings.Add($"{parameterType} {parameterName}");
            }
            return string.Join(", ", parametersAsListOfStrings);
        }

        private void AddFields(SyntaxNode root, Class @class)
        {
            var allReferences = new List<string>();

            var fieldDeclarations = root.DescendantNodes().OfType<FieldDeclarationSyntax>();
            foreach (var fieldDeclarationSyntax in fieldDeclarations)
            {
                var identifierNameSyntax = fieldDeclarationSyntax.Declaration.Type as IdentifierNameSyntax;
                var type = identifierNameSyntax?.Identifier.ToString();
                var name = fieldDeclarationSyntax.Declaration.Variables.ToString();
                if (string.IsNullOrEmpty(type))
                    continue;

                if (allReferences.Contains(type))
                    continue;

                var field = new Field(Guid.NewGuid(), name, @class, $"{type} {name}");
                @class.Fields.Add(field);
                _fieldToType.Add(field.Id, type);

                allReferences.Add(type);
            }

            _classToClass[@class.Id] = _classToClass[@class.Id].Union(allReferences).ToList();
        }

        private void AddProperties(SyntaxNode root, Class @class)
        {
            var allReferences = new List<string>();

            var propertyDeclarations = root.DescendantNodes().OfType<PropertyDeclarationSyntax>();
            foreach (var propertyDeclarationSyntax in propertyDeclarations)
            {
                var identifierNameSyntax = propertyDeclarationSyntax.Type as IdentifierNameSyntax;
                var name = identifierNameSyntax?.Identifier.ToString();
                var type = propertyDeclarationSyntax.Type.ToString();

                if (string.IsNullOrEmpty(name))
                    continue;
                
                if (allReferences.Contains(name))
                    continue;

                var property = new Property(Guid.NewGuid(), name, @class, $"{type} {name}");
                @class.Properties.Add(property);
                _propertyToType.Add(property.Id, type);

                allReferences.Add(type);
            }

            _classToClass[@class.Id] = _classToClass[@class.Id].Union(allReferences).ToList();
        }

        private List<string> GetClassReferences(SyntaxNode syntaxTree, BaseListSyntax baseList)
        {
            var references = new List<string>();

            // Add inheritanced types to references
            if (baseList != null)
            {
                var baseTypes = baseList.Types.ToList();
                references.AddRange(from baseType in baseTypes
                    where baseType != null
                    select baseType.Type.ToString());
            }

            // Add all object creations inside class to references
            var objectCreations = syntaxTree.DescendantNodes().OfType<ObjectCreationExpressionSyntax>();
            foreach (var objectCreationExpressionSyntax in objectCreations)
            {
                var identifierNameSyntax = objectCreationExpressionSyntax.Type as IdentifierNameSyntax;
                var name = identifierNameSyntax?.Identifier.ToString();

                if (string.IsNullOrEmpty(name))
                    continue;

                if (references.Contains(name))
                    continue;

                references.Add(name);
            }
            return references;
        }
    }
}