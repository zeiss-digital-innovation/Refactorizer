using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Refactorizer.VSIX.Models;
using Refactorizer.VSIX.Refactoring.Rewriter;

namespace Refactorizer.VSIX.Refactoring
{
    internal class ClassRefactoring : IRefactoring
    {
        private readonly DTE _dte;

        public ClassRefactoring(DTE dte)
        {
            _dte = dte;
        }

        public async Task<ActionResult> Delete(Class @class)
        { 
            var document = @class.MSDocument;
            var rewriter = new DeleteRewriter();
            var syntaxRoot = await document.GetSyntaxRootAsync();
            var semanticModel = await document.GetSemanticModelAsync();

            int classOrInterfacesDefinitionsInFile
                = @class.IsInterface ? CountInterfaces(syntaxRoot) : CountClasses(syntaxRoot);
            if (classOrInterfacesDefinitionsInFile <= 1)
            {
                RemoveFile(@class);
                return ActionResult.Success;
            }

            SyntaxNode newRoot;
            if (@class.IsInterface)
            {
                var oldNode = GetInterfaceDeclartionSyntax(@class.Name, syntaxRoot, semanticModel);
                if (oldNode == null)
                    return ActionResult.Failed;
                
                var newNode = rewriter.Visit(oldNode);
                newRoot = syntaxRoot.ReplaceNode(oldNode, newNode).NormalizeWhitespace();
            }
            else
            {
                var oldNode = GetClassDeclarationSyntax(@class.Name, syntaxRoot, semanticModel);
                if (oldNode == null)
                    return ActionResult.Failed;

                var newNode = rewriter.Visit(oldNode);
                newRoot = syntaxRoot.ReplaceNode(oldNode, newNode).NormalizeWhitespace();
            }

            try
            {
                File.WriteAllText(@class.Path, newRoot.ToFullString());
            }
            catch (System.Exception)
            {
                return ActionResult.Failed;
            }

            return ActionResult.Success;
        }

        public async Task<ActionResult> Open(Class @class)
        {
            return await Task.Run(() =>
            {
                _dte.MainWindow.Activate();
                _dte.ItemOperations.OpenFile(@class.Path, Constants.vsViewKindTextView);

                return ActionResult.Success;
            });
        }

        private void RemoveFile(Class @class)
        {
            _dte.Solution.FindProjectItem(@class.MSDocument.Name).Delete();
        }

        protected List<InterfaceDeclarationSyntax> GetInterfaceDeclartionSyntaxes(SyntaxNode syntaxRoot)
        {
            return syntaxRoot.DescendantNodesAndSelf().OfType<InterfaceDeclarationSyntax>().ToList();
        }

        protected int CountInterfaces(SyntaxNode syntaxNode)
        {
            return GetInterfaceDeclartionSyntaxes(syntaxNode).Count;
        }

        protected InterfaceDeclarationSyntax GetInterfaceDeclartionSyntax(string className, SyntaxNode syntaxRoot, SemanticModel semanticModel)
        {
            var interfaces = GetInterfaceDeclartionSyntaxes(syntaxRoot);
            foreach (var interfaceDeclarationSyntax in interfaces)
            {
                var symbolInfo = semanticModel.GetSymbolInfo(interfaceDeclarationSyntax).Symbol;
                if (symbolInfo == null)
                    continue;

                if (symbolInfo.Name.Equals(className))
                    return interfaceDeclarationSyntax;
            }

            return null;
        }

        protected List<ClassDeclarationSyntax> GetClassDeclarationSyntaxes(SyntaxNode syntaxRoot)
        {
            var classes = syntaxRoot.DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>().ToList();
            return classes;
        }

        protected int CountClasses(SyntaxNode syntaxNode)
        {
            return GetClassDeclarationSyntaxes(syntaxNode).Count;
        }

        protected ClassDeclarationSyntax GetClassDeclarationSyntax(string className, SyntaxNode syntaxRoot, SemanticModel semanticModel)
        {
            var classes = GetClassDeclarationSyntaxes(syntaxRoot);
            foreach (var classDeclarationSyntax in classes)
            {
                var symbolInfo = semanticModel.GetDeclaredSymbol(classDeclarationSyntax);
                if (symbolInfo == null)
                    continue;

                if (symbolInfo.Name.Equals(className))
                    return classDeclarationSyntax;
            }

            return null;
        }
    }
}
