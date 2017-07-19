using Refactorizer.VSIX.Models;

namespace Refactorizer.VSIX.View
{
    class MethodItemView : DependencyTreeItemView
    {
        public MethodItemView(DependencyTreeItemView parent, IModel relatedModel) : base(parent, relatedModel)
        {
        }

        public override string Name => (RelatedModel as Method)?.Signature ?? RelatedModel.Name;
    }
}
