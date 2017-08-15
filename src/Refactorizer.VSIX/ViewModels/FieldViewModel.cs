using Refactorizer.VSIX.Models;
using Refactorizer.VSIX.Refactorings;

namespace Refactorizer.VSIX.ViewModels
{
    internal class FieldViewModel : DependencyTreeItemViewModel
    {
        public FieldViewModel(SolutionViewModel root, DependencyTreeItemViewModel parent, IModel relatedModel,
            IRefactoringFactory refactoringFactory) : base(root, parent, relatedModel, refactoringFactory)
        {
        }

        public override string Name => (RelatedModel as Field)?.Signature ?? RelatedModel.Name;
    }
}