using Refactorizer.VSIX.Models;
using Refactorizer.VSIX.Refactorings;

namespace Refactorizer.VSIX.ViewModels
{
    internal class FieldViewModel : DependencyTreeItemViewModel
    {
        public FieldViewModel(DependencyTreeItemViewModel parent, IModel relatedModel, IRefactoringFactory refactoringFactory) : base(parent, relatedModel, refactoringFactory)
        {
        }

        public override string Name => (RelatedModel as Field)?.Signature ?? RelatedModel.Name;
    }
}
