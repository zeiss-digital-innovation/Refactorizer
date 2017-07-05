using System;
using System.Linq;
using System.Windows.Media;
using Microsoft.CodeAnalysis;
using Refactorizer.VSIX.Models;
using Project = Refactorizer.VSIX.Models.Project;

namespace Refactorizer.VSIX.ViewModels
{
    class ProjectViewItemViewModel : DependencyTreeViewItemViewModel
    {
        public ProjectViewItemViewModel(DependencyTreeViewItemViewModel parent, IModel relatedModel) : base(parent, relatedModel)
        {
            AddDummy();
        }

        protected override void Loadchildren()
        {
            var project = RelatedModel as Project;
            if (project == null)
                return;

            // Add view model references
            foreach (var ns in project.Namespaces)
                Children.Add(new NamespaceViewItemViewModel(this, ns));
        }
    }
}
