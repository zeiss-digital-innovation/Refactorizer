using Refactorizer.VSIX.Models;
using Refactorizer.VSIX.Refactoring;

namespace Refactorizer.VSIX.ViewModels
{
    internal class PropertyViewModel : DependencyTreeItemViewModel
    {
        public PropertyViewModel(DependencyTreeItemViewModel parent, IModel relatedModel, IRefactoringFactory refactoringFactory) : base(parent, relatedModel, refactoringFactory)
        {
        }

        public override string Name => (RelatedModel as Property)?.Signature ?? RelatedModel.Name;
    }
}
