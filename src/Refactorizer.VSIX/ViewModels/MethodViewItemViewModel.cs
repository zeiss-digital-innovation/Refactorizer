using Refactorizer.VSIX.Models;

namespace Refactorizer.VSIX.ViewModels
{
    class MethodViewItemViewModel : DependencyTreeViewItemViewModel
    {
        public MethodViewItemViewModel(DependencyTreeViewItemViewModel parent, IModel relatedModel) : base(parent, relatedModel)
        {
        }

        public override string Name => (RelatedModel as Method)?.Signature ?? RelatedModel.Name;
    }
}
