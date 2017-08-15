using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Refactorizer.VSIX.Exceptions;
using Refactorizer.VSIX.Models;
using MSDocument = Microsoft.CodeAnalysis.Document;
using MSSolution = Microsoft.CodeAnalysis.Solution;
using MSProject = Microsoft.CodeAnalysis.Project;
using Project = Refactorizer.VSIX.Models.Project;
using Property = Refactorizer.VSIX.Models.Property;
using Solution = Refactorizer.VSIX.Models.Solution;

namespace Refactorizer.VSIX.Analyser
{
    // TODO: Split in multiple classes using CSharpSyntaxWalker
    class CodeAnalyser : ICodeAnalyser
    {
        private Dictionary<Class, List<string>> _classToClassMapping = new Dictionary<Class, List<string>>();

        private Dictionary<Field, string> _fieldToTypeMapping = new Dictionary<Field, string>();

        private Dictionary<Method, List<string>> _methodToClassMapping = new Dictionary<Method, List<string>>();

        private Dictionary<Method, List<string>> _methodToMethodMapping = new Dictionary<Method, List<string>>();

        private Dictionary<Namespace, List<string>> _namespaceToNamespaceMapping =
            new Dictionary<Namespace, List<string>>();

        private Dictionary<Project, List<ProjectId>> _projectToProjectMapping =
            new Dictionary<Project, List<ProjectId>>();

        private Dictionary<Property, string> _propertyToTypeMapping = new Dictionary<Property, string>();

        private readonly SolutionParserBridge _solutionParserBridge;

        public CodeAnalyser(SolutionParserBridge solutionParserBridge)
        {
            _solutionParserBridge = solutionParserBridge;
        }

        /// <summary>
        ///     Generates a new dependency solution based on opened solution
        /// </summary>
        /// <returns></returns>
        public async Task<ISolution> GenerateDependencyTree()
        {
            var solution = new Solution();

            ResetTmpStorage();

            // TODO: Handle all parallel to improve performance
            foreach (var msProject in (await _solutionParserBridge.GetSolution()).Projects)
            {
                var referencedProjectIds = msProject.ProjectReferences.Select(x => x.ProjectId).ToList();
                var project = new Project(msProject.Id.Id, msProject.Name);
                _projectToProjectMapping.Add(project, referencedProjectIds);
                await AddDocuments(msProject, project);

                solution.Projects.Add(project);
            }

            MapReferences(solution);

            return solution;
        }

        private void MapReferences(Solution solution)
        {
            // TODO: Handle all parallel after code above to improve performance
            AddReferencesBetweenProjects(solution);
            AddReferencesBetweenNamespaces(solution);
            AddReferencesBetweenClasses(solution);
            AddReferencesBetweenProperties(solution);
            AddReferencesBetweenFields(solution);
            AddReferencesBetweenMethodsAndClasses(solution);
            AddReferencesBetweenMethods(solution);
        }

        private void ResetTmpStorage()
        {
            _classToClassMapping = new Dictionary<Class, List<string>>();
            _namespaceToNamespaceMapping = new Dictionary<Namespace, List<string>>();
            _projectToProjectMapping = new Dictionary<Project, List<ProjectId>>();
            _fieldToTypeMapping = new Dictionary<Field, string>();
            _propertyToTypeMapping = new Dictionary<Property, string>();
            _methodToMethodMapping = new Dictionary<Method, List<string>>();
            _methodToClassMapping = new Dictionary<Method, List<string>>();
        }

        private void AddReferencesBetweenProjects(Solution solution)
        {
            foreach (var keyValue in _projectToProjectMapping)
            {
                var project = keyValue.Key;
                var references = keyValue.Value;
                foreach (var reference in references)
                {
                    var projectReference = solution.Projects.FirstOrDefault(x => x.Id.Equals(reference.Id));
                    if (projectReference == null)
                        continue;

                    project.OutReferences.Add(projectReference);
                    projectReference.InReferences.Add(project);
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
                    var referenceNamespace = solution.Projects.SelectMany(x => (x as Project).Namespaces).ToList()
                        .FirstOrDefault(x => x.Name.Equals(reference));

                    if (referenceNamespace == null)
                        continue;

                    ns.OutReferences.Add(referenceNamespace);
                    referenceNamespace.InReferences.Add(ns);
                }
            }
        }

        private void AddReferencesBetweenClasses(Solution solution)
        {
            foreach (var keyValue in _classToClassMapping)
            {
                var @class = keyValue.Key;
                var references = keyValue.Value;
                foreach (var referenceFullName in references)
                {
                    var classReference = GetClassByFullName(solution, referenceFullName);

                    if (classReference == null)
                        continue;

                    @class.OutReferences.Add(classReference);
                    classReference.InReferences.Add(@class);
                }
            }
        }

        private void AddReferencesBetweenProperties(Solution solution)
        {
            foreach (var keyValue in _propertyToTypeMapping)
            {
                var property = keyValue.Key;
                var type = keyValue.Value;

                var @class = GetClassByFullName(solution, type);

                if (@class == null)
                    continue;

                property.OutReferences.Add(@class);
                @class.InReferences.Add(property);
            }
        }

        private void AddReferencesBetweenFields(Solution solution)
        {
            foreach (var keyValue in _fieldToTypeMapping)
            {
                var field = keyValue.Key;
                var type = keyValue.Value;

                var @class = GetClassByFullName(solution, type);

                if (@class == null)
                    continue;

                field.OutReferences.Add(@class);
                @class.InReferences.Add(field);
            }
        }

        private void AddReferencesBetweenMethods(Solution solution)
        {
            foreach (var keyValue in _methodToMethodMapping)
            {
                var method = keyValue.Key;
                var referencedMethods = keyValue.Value;

                foreach (var fullMethodName in referencedMethods)
                {
                    var classMethod = solution.Projects.SelectMany(x => (x as Project).Namespaces).ToList()
                        .SelectMany(x => x.Classes).ToList()
                        .SelectMany(x => x.Methods).ToList()
                        .FirstOrDefault(x => x.FullName.Equals(fullMethodName));

                    if (classMethod == null)
                        continue;

                    method.OutReferences.Add(classMethod);
                    classMethod.InReferences.Add(method);
                }
            }
        }

        private void AddReferencesBetweenMethodsAndClasses(Solution solution)
        {
            foreach (var keyValue in _methodToClassMapping)
            {
                var method = keyValue.Key;
                var referencedClasses = keyValue.Value;

                var parentClass = method.Parent as Class;
                var parentNamespace = parentClass?.Parent as Namespace;
                if (parentNamespace == null)
                    continue;

                foreach (var fullClassName in referencedClasses)
                {
                    var @class = GetClassByFullName(solution, fullClassName);
                    if (@class == null)
                        continue;

                    method.OutReferences.Add(@class);
                    @class.InReferences.Add(method);
                }
            }
        }

        private static Class GetClassByFullName(Solution solution, string type)
        {
            var typeReference = solution.Projects.SelectMany(x => (x as Project).Namespaces).ToList().SelectMany(x => x.Classes)
                .ToList().FirstOrDefault(x => x.FullName.Equals(type));
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
            var syntaxRoot = await msDocument.GetSyntaxRootAsync();

            // We need the semanic model to query some informations of nodes
            var semanticModel = await msDocument.GetSemanticModelAsync();

            var referencedNamespaces = GetRelatedNamespaces(syntaxRoot, semanticModel);

            // Use the syntax tree to get all namepace definitions inside
            var namespaces = syntaxRoot.DescendantNodesAndSelf().OfType<NamespaceDeclarationSyntax>();

            foreach (var namespaceDeclarationSyntax in namespaces)
            {
                // Get the symbol for namespace from model to get full name of namespace
                var namespaceSymbol = semanticModel.GetDeclaredSymbol(namespaceDeclarationSyntax);

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
                var classes = namespaceDeclarationSyntax.DescendantNodes().OfType<ClassDeclarationSyntax>();

                // Handle each class declaration inside the namespace
                foreach (var classDeclaration in classes)
                {
                    var symbol = semanticModel.GetDeclaredSymbol(classDeclaration);
                    var baseList = classDeclaration.BaseList;
                    var referencedClasses = GetReturnType(baseList, semanticModel);

                    CreateClass(classDeclaration, symbol, @namespace, referencedClasses, semanticModel, msDocument, false);
                }

                // Use the syntax tree to get all interface declations inside
                var interfaces = namespaceDeclarationSyntax.DescendantNodes().OfType<InterfaceDeclarationSyntax>();

                foreach (var interfaceDeclaration in interfaces)
                {
                    var symbol = semanticModel.GetDeclaredSymbol(interfaceDeclaration);
                    var baseList = interfaceDeclaration.BaseList;
                    var referencedClasses = GetReturnType(baseList, semanticModel);

                    CreateClass(interfaceDeclaration, symbol, @namespace, referencedClasses, semanticModel, msDocument, true);
                }
            }
        }

        private List<string> GetRelatedNamespaces(SyntaxNode syntaxNode, SemanticModel model)
        {
            var usings = syntaxNode.DescendantNodes().OfType<UsingDirectiveSyntax>();

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

        private void CreateClass(SyntaxNode syntaxNode, ISymbol symbol, Namespace @namespace,
            List<string> referencedClasses, SemanticModel model, MSDocument msDocument, bool isInterface)
        {
            var className = symbol.Name;

            var @class = new Class(msDocument.Id.Id, className, @namespace, msDocument.FilePath)
            {
                IsInterface = isInterface
            };
            _classToClassMapping.Add(@class, referencedClasses);

            @namespace.Classes.Add(@class);

            AddFields(syntaxNode, @class, model);
            AddProperties(syntaxNode, @class, model);
            AddMethods(syntaxNode, @class, model);
        }

        // TODO: Handle generics https://stackoverflow.com/questions/43210137/how-to-check-if-method-parameter-type-return-type-is-generic-in-roslyn
        private void AddMethods(SyntaxNode syntaxNode, Class @class, SemanticModel model)
        {
            // Constructors
            var constructorDeclarations = syntaxNode.DescendantNodes().OfType<ConstructorDeclarationSyntax>();
            foreach (var constructorDeclaration in constructorDeclarations)
            {
                var parameterDeclarations = constructorDeclaration.ParameterList.Parameters;

                CreateMethod(constructorDeclaration, parameterDeclarations, @class, model);
            }

            // Methods
            var methodDeclarations = syntaxNode.DescendantNodes().OfType<MethodDeclarationSyntax>();
            foreach (var methodDeclartion in methodDeclarations)
            {
                var returnToken = methodDeclartion.ReturnType;
                var symbol = model.GetSymbolInfo(returnToken).Symbol;
                if (symbol == null)
                    continue;

                var parameterDeclarations = methodDeclartion.ParameterList.Parameters;

                var method = CreateMethod(methodDeclartion, parameterDeclarations, @class, model);
                method.ReturnType = symbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
                _methodToClassMapping[method].Add(symbol.ToDisplayString());
            }
        }

        private Method CreateMethod(SyntaxNode syntaxNode, SeparatedSyntaxList<ParameterSyntax> parameterDeclarations, Class @class, SemanticModel model)
        {
            var walker = new MethodSyntaxWalker(model);
            walker.Visit(syntaxNode);

            var parametersAsListOfStrings = new List<string>();
            foreach (var parameter in parameterDeclarations)
            {
                var parameterName = parameter.Identifier.ToString();
                var parameterType = parameter.Type.ToString();
                walker.ClassReferences.Add(parameterType);

                parametersAsListOfStrings.Add($"{parameterType} {parameterName}");
            }
            var symbol = model.GetDeclaredSymbol(syntaxNode);

            var method =
                new Method(Guid.NewGuid(), symbol.Name, symbol.ToDisplayString(), @class)
                {
                    Parameter = string.Join(", ", parametersAsListOfStrings)
                };

            @class.Methods.Add(method);
            _methodToClassMapping.Add(method, walker.ClassReferences);
            _methodToMethodMapping.Add(method, walker.MethodReferences);

            return method;
        }

        private void AddFields(SyntaxNode root, Class @class, SemanticModel model)
        {
            var allReferences = new List<string>();

            var fieldDeclarations = root.DescendantNodes().OfType<FieldDeclarationSyntax>();
            foreach (var fieldDeclarationSyntax in fieldDeclarations)
            {
                var symbol = model.GetSymbolInfo(fieldDeclarationSyntax.Declaration.Type).Symbol;
                if (symbol == null)
                    continue;

                var type = symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
                var typeWithoutNs = type.Split('.').Last();
                var name = fieldDeclarationSyntax.Declaration.Variables.ToString();
                if (string.IsNullOrEmpty(type))
                    continue;

                if (allReferences.Contains(type))
                    continue;

                var field = new Field(Guid.NewGuid(), name, @class, $"{typeWithoutNs} {name}");
                @class.Fields.Add(field);
                _fieldToTypeMapping.Add(field, type);

                allReferences.Add(type);
            }

            _classToClassMapping[@class] = _classToClassMapping[@class].Union(allReferences).ToList();
        }

        private void AddProperties(SyntaxNode root, Class @class, SemanticModel model)
        {
            var allReferences = new List<string>();

            var propertyDeclarations = root.DescendantNodes().OfType<PropertyDeclarationSyntax>();
            foreach (var propertyDeclarationSyntax in propertyDeclarations)
            {
                var identifierNameSyntax = propertyDeclarationSyntax.Type as IdentifierNameSyntax;
                var name = identifierNameSyntax?.Identifier.ToString();
                var symbol = model.GetSymbolInfo(propertyDeclarationSyntax.Type).Symbol;
                if (symbol == null)
                    continue;

                var type = symbol.ToDisplayString();
                var typeWithoutNs = type.Split('.').Last();

                if (string.IsNullOrEmpty(name))
                    continue;

                if (allReferences.Contains(name))
                    continue;

                var property = new Property(Guid.NewGuid(), name, @class, $"{typeWithoutNs} {name}");
                @class.Properties.Add(property);
                _propertyToTypeMapping.Add(property, type);

                allReferences.Add(type);
            }

            _classToClassMapping[@class] = _classToClassMapping[@class].Union(allReferences).ToList();
        }

        private List<string> GetReturnType(BaseListSyntax baseList, SemanticModel semanticModel)
        {
            var references = new List<string>();

            // Add inheritanced types to references
            if (baseList == null)
                return references;

            var baseTypes = baseList.Types.Select(x => x.Type).ToList();
            foreach (var type in baseTypes)
            {
                var symbol = semanticModel.GetSymbolInfo(type).Symbol;
                if (symbol == null)
                    continue;
                
                references.Add(symbol.ToDisplayString());
            }

            return references;
        }
    }
}