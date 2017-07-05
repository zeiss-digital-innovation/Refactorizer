using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Refactorizer.VSIX.Models;

namespace Refactorizer.VSIX.ViewModels
{
    internal class SolutionViewModel 
    {
        public SolutionViewModel(Solution solution)
        {
            // Create view model childs using data model
            Projects = new ReadOnlyCollection<ProjectViewItemViewModel>((from project in solution.Projects select new ProjectViewItemViewModel(null, project)).ToList());
        }

        public IReadOnlyCollection<ProjectViewItemViewModel> Projects { get; }
    }
}