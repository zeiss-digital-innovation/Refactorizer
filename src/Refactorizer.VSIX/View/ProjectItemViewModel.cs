using System;
using System.Linq;
using System.Windows.Media;
using Microsoft.CodeAnalysis;
using Refactorizer.VSIX.Models;
using Project = Refactorizer.VSIX.Models.Project;

namespace Refactorizer.VSIX.ViewModels
{
    class ProjectItemViewModel : TreeItemViewModel
    {
        public ProjectItemViewModel(TreeItemViewModel parent, IModel relatedModel) : base(parent, relatedModel)
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
                Children.Add(new NamespaceItemViewModel(this, ns));

            // Add referenced view models if exists
            foreach (var viewModel in Children)
            {
                if (!(viewModel is NamespaceItemViewModel))
                    continue;

                var dataModel = viewModel.RelatedModel;
                foreach (var dataModelReference in dataModel.References)
                {
                    var selectedNamespace = Children.FirstOrDefault(x => x.RelatedModel.Id.Equals(dataModelReference.Id));
                    if (selectedNamespace != null)
                        viewModel.References.Add(selectedNamespace);
                }
            }
        }
    }
}
