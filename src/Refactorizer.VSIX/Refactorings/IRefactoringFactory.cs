using Refactorizer.VSIX.ViewModels;

namespace Refactorizer.VSIX.Refactorings
{
    public interface IRefactoringFactory
    {
        IRefactoring GetRefactoringForViewModel(IDependencyTreeItemViewModel viewModel);
    }
}