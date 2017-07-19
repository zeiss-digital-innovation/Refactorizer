using Refactorizer.VSIX.Models;

namespace Refactorizer.VSIX.View
{
    class PropertyItemView : DependencyTreeItemView
    {
        public PropertyItemView(DependencyTreeItemView parent, IModel relatedModel) : base(parent, relatedModel)
        {
        }

        public override string Name => (RelatedModel as Property)?.Signature ?? RelatedModel.Name;
    }
}
