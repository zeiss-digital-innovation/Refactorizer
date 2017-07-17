using Refactorizer.VSIX.Models;

namespace Refactorizer.VSIX.View
{
    class NamespaceItemView : DependencyTreeItemView
    {

        public NamespaceItemView(DependencyTreeItemView parent, IModel relatedModel) : base(parent, relatedModel)
        {
        }

        public override void Loadchildren()
        {
            var @namespace = RelatedModel as Namespace;
            if (@namespace == null)
                return;

            foreach (var @class in @namespace.Classes)
                Children.Add(new ClassItemView(this, @class));
        }
    }
}