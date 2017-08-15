using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Refactorizer.VSIX.Analyser;
using Refactorizer.VSIX.Models;
using Refactorizer.VSIX.Refactorings.Rewriter;

namespace Refactorizer.VSIX.Refactorings
{
    internal class MethodRefactoring : RefactoringBase, IRefactoring
    {
        private readonly SolutionParserBridge _solutionParserBridge;

        public MethodRefactoring(SolutionParserBridge solutionParserBridge)
        {
            _solutionParserBridge = solutionParserBridge;
        }

        public async Task<ActionResult> Delete(Method method)
        {
            var @class = method.Parent as Class;
            if (@class == null)
                return ActionResult.Failed;

            var rewriter = new DeleteRewriter();
            var document = await _solutionParserBridge.GetDocument(@class);
            var syntaxNode = await document.GetSyntaxRootAsync();
            var semanticModel = await document.GetSemanticModelAsync();

            var oldNode = GetMethodDeclarationSyntax(method.Name, syntaxNode, semanticModel);
            if (oldNode == null)
                return ActionResult.Failed;
            
            var newNode = rewriter.Visit(oldNode);
            var newRoot = syntaxNode.ReplaceNode(oldNode, newNode).NormalizeWhitespace();

            return ApplyNewRootToClass(@class, newRoot);
        }

        public async Task<ActionResult> Rename(Method method, string newName)
        {
            var @class = method.Parent as Class;
            if (@class == null)
                return ActionResult.Failed;

            var document = await _solutionParserBridge.GetDocument(@class);
            var syntaxNode = await document.GetSyntaxRootAsync();
            var semanticModel = await document.GetSemanticModelAsync();

            var methodNode = GetMethodDeclarationSyntax(method.Name, syntaxNode, semanticModel);
            var symbol = semanticModel.GetDeclaredSymbol(methodNode);

            return await RenameSymbol(newName, document, symbol);
        }

        private MethodDeclarationSyntax GetMethodDeclarationSyntax(string methodName, SyntaxNode syntaxRoot, SemanticModel semanticModel)
        {
            var methodDeclarationSyntaxs = syntaxRoot.DescendantNodes().OfType<MethodDeclarationSyntax>();
            foreach (var methodDeclarationSyntax in methodDeclarationSyntaxs)
            {
                var symbol = semanticModel.GetDeclaredSymbol(methodDeclarationSyntax);
                if (symbol == null)
                    continue;

                if (symbol.Name.Equals(methodName))
                    return methodDeclarationSyntax;
            }

            return null;
        }
    }
}