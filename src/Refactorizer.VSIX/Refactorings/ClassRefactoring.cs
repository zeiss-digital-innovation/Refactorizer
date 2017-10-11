using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Refactorizer.VSIX.Analyser;
using Refactorizer.VSIX.Models;
using Refactorizer.VSIX.Refactorings.Rewriter;

namespace Refactorizer.VSIX.Refactorings
{
    internal class ClassRefactoring : RefactoringBase, IRefactoring
    {
        private readonly DTE _dte;
        private readonly SolutionParserBridge _solutionParserBridge;

        public ClassRefactoring(DTE dte, SolutionParserBridge solutionParserBridge)
        {
            _dte = dte;
            _solutionParserBridge = solutionParserBridge;
        }

        public async Task<ActionResult> Delete(Class @class)
        {
            var document = await _solutionParserBridge.GetDocument(@class);
            var syntaxRoot = await document.GetSyntaxRootAsync();

            // Only one class/interface in this file
            // No need to change the syntax, delete the file
            int classOrInterfacesDefinitionsInFile
                = @class.IsInterface ? CountInterfaces(syntaxRoot) : CountClasses(syntaxRoot);
            if (classOrInterfacesDefinitionsInFile <= 1)
            {
                await RemoveFile(@class);
                return ActionResult.Success;
            }

            return await UseRewriter(@class, new DeleteRewriter());
        }

        public async Task<ActionResult> Rename(Class @class, string newName)
        {
            var document = await _solutionParserBridge.GetDocument(@class);
            var syntaxRoot = await document.GetSyntaxRootAsync();
            var semanticModel = await document.GetSemanticModelAsync();

            var symbol = @class.IsInterface 
                ? semanticModel.GetSymbolInfo(GetInterfaceDeclartionSyntax(@class.Name, syntaxRoot, semanticModel)).Symbol 
                : ModelExtensions.GetDeclaredSymbol(semanticModel, GetClassDeclarationSyntax(@class.Name, syntaxRoot, semanticModel));

            return await RenameSymbol(newName, document, symbol);
        }

        private async Task<ActionResult> UseRewriter(Class @class, CSharpSyntaxRewriter rewriter)
        {
            var document = await _solutionParserBridge.GetDocument(@class);
            var syntaxRoot = await document.GetSyntaxRootAsync();
            var semanticModel = await document.GetSemanticModelAsync();

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

            return ApplyNewRootToClass(@class, newRoot);
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

        private async Task RemoveFile(Class @class)
        {
            var document = await _solutionParserBridge.GetDocument(@class);
            _dte.Solution.FindProjectItem(document.Name).Delete();
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
                var symbolInfo = ModelExtensions.GetDeclaredSymbol(semanticModel, classDeclarationSyntax);
                if (symbolInfo == null)
                    continue;

                if (symbolInfo.Name.Equals(className))
                    return classDeclarationSyntax;
            }

            return null;
        }
    }
}
