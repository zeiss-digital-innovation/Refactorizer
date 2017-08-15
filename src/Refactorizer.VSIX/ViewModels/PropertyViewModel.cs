using Refactorizer.VSIX.Models;
using Refactorizer.VSIX.Refactorings;

namespace Refactorizer.VSIX.ViewModels
{
    internal class PropertyViewModel : DependencyTreeItemViewModel
    {
        public PropertyViewModel(SolutionViewModel root, DependencyTreeItemViewModel parent, IModel relatedModel,
            IRefactoringFactory refactoringFactory) : base(root, parent, relatedModel, refactoringFactory)
        {
        }

        public override string Name => (RelatedModel as Property)?.Signature ?? RelatedModel.Name;
    }
}