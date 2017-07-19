using Refactorizer.VSIX.Models;

namespace Refactorizer.VSIX.View
{
    class ClassItemView : DependencyTreeItemView
    {
        public ClassItemView(DependencyTreeItemView parent, IModel relatedModel) : base(parent, relatedModel)
        {
            var @class = RelatedModel as Class;
            if (@class == null)
                return;

            foreach (var field in @class.Fields)
                Children.Add(new FieldItemView(this, field));
            
            foreach (var property in @class.Properties)
                Children.Add(new PropertyItemView(this, property));

            foreach (var method in @class.Methods)
                Children.Add(new MethodItemView(this, method));
        }
    }
}