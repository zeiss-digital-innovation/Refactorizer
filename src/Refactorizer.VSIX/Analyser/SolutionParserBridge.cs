using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
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

namespace Refactorizer.VSIX.Analyser
{
    class SolutionParserBridge
    {
        private readonly DTE _dte;

        public SolutionParserBridge(DTE dte)
        {
            _dte = dte;
        }

        public async Task<MSSolution> GetSolution()
        {
            var solutionFullName = _dte.Solution.FullName;
            if (string.IsNullOrEmpty(solutionFullName))
                throw new NoSolutionOpenException();

            var msBuildWorkspace = MSBuildWorkspace.Create();
            return await msBuildWorkspace.OpenSolutionAsync(solutionFullName);
        }

        public async Task<MSDocument> GetDocument(Class @class)
        {
            var solution = await GetSolution();
            var oldNamespace = (Namespace) @class.Parent;
            var oldProject = (Project) oldNamespace.Parent;

            // Get the correct project
            var project = solution.Projects.FirstOrDefault(x => x.Name.Equals(oldProject.Name));
            if (project == null)
                return null;

            foreach (var document in project.Documents)
            {
                var semanticModel = await document.GetSemanticModelAsync();
                var syntaxNode = await document.GetSyntaxRootAsync();

                var namespaces = syntaxNode.DescendantNodesAndSelf().OfType<NamespaceDeclarationSyntax>().ToList();
                NamespaceDeclarationSyntax namespaceDeclarationSyntax = null;
                foreach (var declarationSyntax in namespaces)
                {
                    var symbol = semanticModel.GetDeclaredSymbol(declarationSyntax);
                    if (symbol == null)
                        continue;

                    if (!symbol.ToDisplayString().Equals(oldNamespace.Name))
                        continue;

                    namespaceDeclarationSyntax = declarationSyntax;
                }

                // Skip all incorrect namespaces
                if(namespaceDeclarationSyntax == null)
                    continue;

                if (@class.IsInterface)
                {
                    var interfaces = namespaceDeclarationSyntax.DescendantNodes().OfType<InterfaceDeclarationSyntax>().ToList();
                    if (interfaces.Select(interfaceDeclarationSyntax => semanticModel.GetDeclaredSymbol(interfaceDeclarationSyntax))
                        .Where(symbol => symbol != null)
                        .Any(symbol => symbol.Name.Equals(@class.Name)))
                    {
                        return document;
                    }
                }
                else
                {
                    var classes = namespaceDeclarationSyntax.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
                    if (classes.Select(classDeclarationSyntax => semanticModel.GetDeclaredSymbol(classDeclarationSyntax))
                        .Where(symbol => symbol != null)
                        .Any(symbol => symbol.Name.Equals(@class.Name)))
                    {
                        return document;
                    }
                }
            }

            return null;
        }
    }
}
