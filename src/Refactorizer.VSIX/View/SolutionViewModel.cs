using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Refactorizer.VSIX.Models;

namespace Refactorizer.VSIX.View
{
    internal class SolutionViewModel 
    {
        public SolutionViewModel(Solution solution)
        {
            // Create view model childs using data model
            Projects = new ReadOnlyCollection<ProjectItemView>((from project in solution.Projects select new ProjectItemView(null, project)).ToList());
        }

        public IReadOnlyCollection<ProjectItemView> Projects { get; }
    }
}