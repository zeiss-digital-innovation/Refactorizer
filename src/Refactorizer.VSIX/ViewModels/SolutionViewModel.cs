using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Refactorizer.VSIX.Models;
using Refactorizer.VSIX.Refactorings;

namespace Refactorizer.VSIX.ViewModels
{
    internal class SolutionViewModel 
    {
        public SolutionViewModel(ISolution solution, IRefactoringFactory refactoringFactory)
        {
            // Create view model childs using data model
            Projects = new ReadOnlyCollection<ProjectViewModel>((from project in solution.Projects select new ProjectViewModel(null, project, refactoringFactory)).ToList());
        }

        public IReadOnlyCollection<ProjectViewModel> Projects { get; }
    }
}