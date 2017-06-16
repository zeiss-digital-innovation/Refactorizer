using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Refactorizer.VSIX.Models;

namespace Refactorizer.VSIX.ViewModels
{
    internal class SolutionViewModel 
    {
        public SolutionViewModel(Solution solution)
        {
            Projects = new ReadOnlyCollection<ProjectViewItemViewModel>(
                (from project in solution.Projects
                    select new ProjectViewItemViewModel(project, null)
                ).ToList());
        }

        public IReadOnlyCollection<ProjectViewItemViewModel> Projects { get; }
    }
}