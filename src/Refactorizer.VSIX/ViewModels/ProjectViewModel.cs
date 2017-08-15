﻿using Refactorizer.VSIX.Models;
using Refactorizer.VSIX.Refactorings;
using Project = Refactorizer.VSIX.Models.Project;

namespace Refactorizer.VSIX.ViewModels
{
    internal class ProjectViewModel : DependencyTreeItemViewModel
    {
        public ProjectViewModel(SolutionViewModel root, DependencyTreeItemViewModel parent, IModel relatedModel,
            IRefactoringFactory refactoringFactory) : base(root, parent, relatedModel, refactoringFactory)
        {
            var project = RelatedModel as Project;
            if (project == null)
                return;

            // Add view model references
            foreach (var ns in project.Namespaces)
                Children.Add(new NamespaceViewModel(root, this, ns, refactoringFactory));
        }
    }
}