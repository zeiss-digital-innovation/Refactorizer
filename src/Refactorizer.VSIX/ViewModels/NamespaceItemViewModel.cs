using System;
using Refactorizer.VSIX.Models;

namespace Refactorizer.VSIX.ViewModels
{
    class NamespaceItemViewModel : TreeItemViewModel
    {

        public NamespaceItemViewModel(TreeItemViewModel parent, IModel relatedModel) : base(parent, relatedModel)
        {
            AddDummy();
        }

        protected override void Loadchildren()
        {
            var @namespace = RelatedModel as Namespace;
            if (@namespace == null)
                return;

            foreach (var @class in @namespace.Classes)
                Children.Add(new ClassItemViewModel(this, @class));
                
            // TODO: Add references between TreeViewItems using the id (Guid)
        }
    }
}