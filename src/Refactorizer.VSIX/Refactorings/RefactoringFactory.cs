using EnvDTE;
using Refactorizer.VSIX.ViewModels;

namespace Refactorizer.VSIX.Refactorings
{
    internal class RefactoringFactory : IRefactoringFactory
    {
        private readonly DTE _dte;

        public RefactoringFactory(DTE dte)
        {
            _dte = dte;
        }

        public IRefactoring GetRefactoringForViewModel(IDependencyTreeItemViewModel viewModel)
        {
            if (viewModel is ClassViewModel)
                return new ClassRefactoring(_dte);

            if (viewModel is FieldViewModel)
                return new FieldRefactoring();

            if (viewModel is MethodViewModel)
                return new MethodRefactoring();
            
            if (viewModel is NamespaceViewModel)
            {
                var classRefactoring = new ClassRefactoring(_dte);
                return new NamespaceRefactoring(classRefactoring);
            }

            if (viewModel is ProjectViewModel)
                return new ProjectRefactoring();

            if (viewModel is PropertyViewModel)
                return new PropertyRefactioring();

            return null;
        }
    }
}
