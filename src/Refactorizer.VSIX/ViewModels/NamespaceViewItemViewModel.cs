using System;
using Refactorizer.VSIX.Models;

namespace Refactorizer.VSIX.ViewModels
{
    class NamespaceViewItemViewModel : DependencyTreeViewItemViewModel
    {

        public NamespaceViewItemViewModel(DependencyTreeViewItemViewModel parent, IModel relatedModel) : base(parent, relatedModel)
        {
            AddDummy();
        }

        public override void Loadchildren()
        {
            var @namespace = RelatedModel as Namespace;
            if (@namespace == null)
                return;

            foreach (var @class in @namespace.Classes)
                Children.Add(new ClassViewItemViewModel(this, @class));
        }
    }
}