using Refactorizer.VSIX.Models;
using Project = Refactorizer.VSIX.Models.Project;

namespace Refactorizer.VSIX.View
{
    class ProjectItemView : DependencyTreeItemView
    {
        public ProjectItemView(DependencyTreeItemView parent, IModel relatedModel) : base(parent, relatedModel)
        {
        }

        public override void Loadchildren()
        {
            var project = RelatedModel as Project;
            if (project == null)
                return;

            // Add view model references
            foreach (var ns in project.Namespaces)
                Children.Add(new NamespaceItemView(this, ns));
        }
    }
}
