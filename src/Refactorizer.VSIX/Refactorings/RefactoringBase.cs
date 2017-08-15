using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Rename;
using Refactorizer.VSIX.Models;

namespace Refactorizer.VSIX.Refactorings
{
    internal abstract class RefactoringBase
    {
        protected async Task<ActionResult> RenameSymbol(string newName, Document document, ISymbol symbol)
        {
            var solution = document.Project.Solution;
            var optionSet = solution.Workspace.Options;
            var newSolution = await Renamer.RenameSymbolAsync(solution, symbol, newName, optionSet);

            return !solution.Workspace.TryApplyChanges(newSolution) ? ActionResult.Failed : ActionResult.Success;
        }

        protected ActionResult ApplyNewRootToClass(Class @class, SyntaxNode newRoot)
        {
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
    }
}