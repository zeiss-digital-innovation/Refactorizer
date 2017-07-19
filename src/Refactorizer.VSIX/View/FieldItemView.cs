using Refactorizer.VSIX.Models;

namespace Refactorizer.VSIX.View
{
    class FieldItemView : DependencyTreeItemView
    {
        public FieldItemView(DependencyTreeItemView parent, IModel relatedModel) : base(parent, relatedModel)
        {
        }

        public override string Name => (RelatedModel as Field)?.Signature ?? RelatedModel.Name;
    }
}
