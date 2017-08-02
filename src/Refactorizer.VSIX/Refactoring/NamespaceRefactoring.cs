using System.Threading.Tasks;
using Refactorizer.VSIX.Models;

namespace Refactorizer.VSIX.Refactoring
{
    internal class NamespaceRefactoring : IRefactoring
    {
        private readonly ClassRefactoring _classRefactoring;

        public NamespaceRefactoring(ClassRefactoring classRefactoring)
        {
            _classRefactoring = classRefactoring;
        }

        public async Task<ActionResult> Delete(Namespace ns)
        {
            var result = ActionResult.Success;
            foreach (var @class in ns.Classes)
            {
                var actionResult = await _classRefactoring.Delete(@class);
                if (actionResult == ActionResult.Failed)
                    result = ActionResult.Failed;
            }

            return result;
        }
    }
}