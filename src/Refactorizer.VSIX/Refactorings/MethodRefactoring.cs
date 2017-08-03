using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Refactorizer.VSIX.Models;
using Refactorizer.VSIX.Refactorings.Rewriter;

namespace Refactorizer.VSIX.Refactorings
{
    internal class MethodRefactoring : IRefactoring
    {
        public async Task<ActionResult> Delete(Method method)
        {
            var @class = method.Parent as Class;
            if (@class == null)
                return ActionResult.Failed;

            var rewriter = new DeleteRewriter();
            var document = @class.MSDocument;
            var syntaxRoot = await document.GetSyntaxRootAsync();
            var semanticModel = await document.GetSemanticModelAsync();

            var oldNode = GetMethodDeclarationSyntax(method.Name, syntaxRoot, semanticModel);
            if (oldNode == null)
                return ActionResult.Failed;
            
            var newNode = rewriter.Visit(oldNode);
            var newRoot = syntaxRoot.ReplaceNode(oldNode, newNode).NormalizeWhitespace();

            try
            {
                File.WriteAllText(@class.Path, newRoot.ToFullString());
            }
            catch (System.Exception e)
            {
                return ActionResult.Failed;
            }

            return ActionResult.Success;
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