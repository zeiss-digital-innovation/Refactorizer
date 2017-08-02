using Refactorizer.VSIX.ViewModels;

namespace Refactorizer.VSIX.Refactoring
{
    public interface IRefactoringFactory
    {
        IRefactoring GetRefactoringForViewModel(IDependencyTreeItemViewModel viewModel);
    }
}