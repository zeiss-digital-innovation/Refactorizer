using System;
using Refactorizer.VSIX.Models;

namespace Refactorizer.VSIX.ViewModels
{
    class ClassViewItemViewModel : DependencyTreeViewItemViewModel
    {
        public ClassViewItemViewModel(DependencyTreeViewItemViewModel parent, IModel relatedModel) : base(parent, relatedModel)
        {
        }

        public override void Loadchildren()
        {
            var @class = RelatedModel as Class;
            if (@class == null)
                return;

            foreach (var field in @class.Fields)
                Children.Add(new FieldViewItemViewModel(this, field));
            
            foreach (var property in @class.Properties)
                Children.Add(new PropertyViewItemViewModel(this, property));

            foreach (var method in @class.Methods)
                Children.Add(new MethodViewItemViewModel(this, method));
        }
    }
}