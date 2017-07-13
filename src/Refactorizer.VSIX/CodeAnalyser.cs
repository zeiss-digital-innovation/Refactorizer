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
        private Dictionary<Method, List<string>> _methodToClassMapping = new Dictionary<Method, List<string>>();

        private Dictionary<Method, List<string>> _methodToMethodMapping = new Dictionary<Method, List<string>>();

        private Dictionary<Property, string> _propertyToTypeMapping = new Dictionary<Property, string>();

        private Dictionary<Field, string> _fieldToTypeMapping = new Dictionary<Field, string>();

        private Dictionary<Class, List<string>> _classToClassMapping = new Dictionary<Class, List<string>>();

        private Dictionary<Class, List<string>> _classToNamespaceMapping = new Dictionary<Class, List<string>>();

        private Dictionary<Namespace, List<string>> _namespaceToNamespaceMapping = new Dictionary<Namespace, List<string>>();

        private Dictionary<Project, List<ProjectId>> _projectToProjectMapping = new Dictionary<Project, List<ProjectId>>();

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
            _classToClassMapping = new Dictionary<Class, List<string>>();
            _classToNamespaceMapping = new Dictionary<Class, List<string>>();
            _namespaceToNamespaceMapping = new Dictionary<Namespace, List<string>>();
            _projectToProjectMapping = new Dictionary<Project, List<ProjectId>>();
            _fieldToTypeMapping = new Dictionary<Field, string>();
            _propertyToTypeMapping = new Dictionary<Property, string>();
            _methodToMethodMapping = new Dictionary<Method, List<string>>();
            _methodToClassMapping = new Dictionary<Method, List<string>>();

            // TODO: Handle all parallel to improve performance
            foreach (var msProject in (await GetCurrentSolution()).Projects)
            {
                var referencedProjectIds = msProject.ProjectReferences.Select(x => x.ProjectId).ToList();
                var project = new Project(Guid.NewGuid(), msProject.Id, msProject.Name);
                _projectToProjectMapping.Add(project, referencedProjectIds);
                await AddDocuments(msProject, project);

                solution.Projects.Add(project);
            }

            // TODO: Handle all parallel after code above to improve performance
            AddReferencesBetweenProjects(solution);
            AddReferencesBetweenNamespaces(solution);
            AddReferencesBetweenClasses(solution);
            AddReferencesBetweenProperties();
            AddReferencesBetweenFields();
            AddReferencesBetweenMethodsAndClasses();
            AddReferencesBetweenMethods();

            return solution;
        }

        private void AddReferencesBetweenProjects(Solution solution)
        {
            foreach (var keyValue in _projectToProjectMapping)
            {
                var project = keyValue.Key;
                var references = keyValue.Value;
                foreach (var reference in references)
                {
                    var projectReference = solution.Projects.FirstOrDefault(x => x.ProjectId.Equals(reference));
                    if (projectReference != null)
                        project.References.Add(projectReference);
                }
            }
        }

        private void AddReferencesBetweenNamespaces(Solution solution)
        {
            foreach (var keyValue in _namespaceToNamespaceMapping)
            {
                var ns = keyValue.Key;
                var references = keyValue.Value;
                foreach (var reference in references)
                {
                    // TODO: This does not work if a namespace is defined in multiple projects
                    // At this case we need to check the referenced classes
                    var referenceNamespace = solution.Projects.SelectMany(x => x.Namespaces).ToList()
                        .FirstOrDefault(x => x.Name.Equals(reference));

                    if (referenceNamespace != null)
                        ns.References.Add(referenceNamespace);
                }
            }
        }

        private void AddReferencesBetweenClasses(Solution solution)
        {
            foreach (var keyValue in _classToClassMapping)
            {
                var @class = keyValue.Key;
                var references = keyValue.Value;
                foreach (var reference in references)
                {
                    var namespaceAsStringOfClass = _classToNamespaceMapping.FirstOrDefault(x => x.Key.Id.Equals(@class.Id)).Value;

                    var namespacesOfClass = solution.Projects.SelectMany(x => x.Namespaces).ToList()
                        .Where(x => namespaceAsStringOfClass.Contains(x.Name)).ToList();

                    var classReference = namespacesOfClass.SelectMany(x => x.Classes).ToList()
                        .FirstOrDefault(x => x.Name.Equals(reference));

                    if (classReference != null)
                        @class.References.Add(classReference);
                }
            }
        }

        private void AddReferencesBetweenProperties()
        {
            foreach (var keyValue in _propertyToTypeMapping)
            {
                var property = keyValue.Key;
                var type = keyValue.Value;

                var typeReference = GetClassByType(property.Parent as Class, type);

                if (typeReference == null)
                    continue;

                property.References.Add(typeReference);
            }
        }


        private void AddReferencesBetweenFields()
        {
            foreach (var keyValue in _fieldToTypeMapping)
            {
                var field = keyValue.Key;
                var type = keyValue.Value;

                var typeReference = GetClassByType(field.Parent as Class, type);

                if (typeReference == null)
                    continue;
                
                field.References.Add(typeReference);
            }
        }

        private void AddReferencesBetweenMethods()
        {
            throw new NotImplementedException();
        }

        private void AddReferencesBetweenMethodsAndClasses()
        {
            throw new NotImplementedException();
        }

        private static Class GetClassByType(Class classThatContainsProperty, string type)
        {
            var typeReference = classThatContainsProperty?.ReferencedNamespaces.SelectMany(ns => ns.Classes)
                .ToList()
                .FirstOrDefault(@class => @class.Name.Equals(type));
            return typeReference;
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
                    _namespaceToNamespaceMapping.Add(@namespace, referencedNamespaces);
                }
                else
                {
                    _namespaceToNamespaceMapping[@namespace] = _namespaceToNamespaceMapping[@namespace]
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

                    CreateClass(root, symbol, @namespace, referencedClasses, referencedNamespaces);
                }

                // Use the syntax tree to get all interface declations inside
                var interfaces = namespaceSyntaxRoot.DescendantNodes().OfType<InterfaceDeclarationSyntax>();

                foreach (var interfaceDeclaration in interfaces)
                {
                    var symbol = model.GetDeclaredSymbol(interfaceDeclaration);
                    var baseList = interfaceDeclaration.BaseList;
                    var root = await interfaceDeclaration.SyntaxTree.GetRootAsync();
                    var referencedClasses = GetClassReferences(root, baseList);

                    CreateClass(root, symbol, @namespace, referencedClasses, referencedNamespaces);
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

        private void CreateClass(SyntaxNode root, ISymbol symbol, Namespace @namespace, List<string> referencedClasses, List<string> referencedNamespaces)
        {
            var className = symbol.Name;

            var @class = new Class(Guid.NewGuid(), className, @namespace);
            _classToClassMapping.Add(@class, referencedClasses);
            _classToNamespaceMapping.Add(@class, referencedNamespaces);

            @namespace.Classes.Add(@class);

            AddFields(root, @class);
            AddProperties(root, @class);
            AddMethods(root, @class);
        }

        // TODO: Handle generics https://stackoverflow.com/questions/43210137/how-to-check-if-method-parameter-type-return-type-is-generic-in-roslyn
        private void AddMethods(SyntaxNode syntaxTree, Class @class)
        {
            // Constructors
            var constructorDeclarations = syntaxTree.DescendantNodes().OfType<ConstructorDeclarationSyntax>();
            foreach (var constructorDeclaration in constructorDeclarations)
            {
                var methodName = constructorDeclaration.Identifier.ToString();
                var parameterDeclarations = constructorDeclaration.ParameterList.Parameters;

                CreateMethod($"{methodName}", parameterDeclarations, @class);
            }

            // Methods
            var methodDeclarations = syntaxTree.DescendantNodes().OfType<MethodDeclarationSyntax>();
            foreach (var methodDeclartion in methodDeclarations)
            {
                var methodName = methodDeclartion.Identifier.ToString();
                var returnType = methodDeclartion.ReturnType.ToString();
                var parameterDeclarations = methodDeclartion.ParameterList.Parameters;

                CreateMethod($"{methodName}", parameterDeclarations, @class, returnType);
            }
        }

        private void CreateMethod(string methodName, SeparatedSyntaxList<ParameterSyntax> parameterDeclarations, Class @class, string returnType = null)
        {
            var methodToClassReferences = new List<string>();
            var parametersAsListOfStrings = new List<string>();
            foreach (var parameter in parameterDeclarations)
            {
                var parameterName = parameter.Identifier.ToString();
                var parameterType = parameter.Type.ToString();
                methodToClassReferences.Add(parameterType);

                parametersAsListOfStrings.Add($"{parameterType} {parameterName}");
            }

            var method = new Method(Guid.NewGuid(), methodName, @class) {Parameter = string.Join(", ", parametersAsListOfStrings)};
            if (returnType != null)
                method.ReturnType = returnType;

            @class.Methods.Add(method);
            _methodToClassMapping.Add(method, methodToClassReferences);
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
                _fieldToTypeMapping.Add(field, type);

                allReferences.Add(type);
            }

            _classToClassMapping[@class] = _classToClassMapping[@class].Union(allReferences).ToList();
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
                _propertyToTypeMapping.Add(property, type);

                allReferences.Add(type);
            }

            _classToClassMapping[@class] = _classToClassMapping[@class].Union(allReferences).ToList();
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

            return references;
        }
    }
}