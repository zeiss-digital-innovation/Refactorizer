using EnvDTE;
using Refactorizer.VSIX.Analyser;
using Refactorizer.VSIX.ViewModels;

namespace Refactorizer.VSIX.Refactorings
{
    internal class RefactoringFactory : IRefactoringFactory
    {
        private readonly DTE _dte;

        private readonly SolutionParserBridge _solutionParserBridge;

        public RefactoringFactory(DTE dte, SolutionParserBridge solutionParserBridge)
        {
            _dte = dte;
            _solutionParserBridge = solutionParserBridge;
        }

        public IRefactoring GetRefactoringForViewModel(IDependencyTreeItemViewModel viewModel)
        {
            if (viewModel is ClassViewModel)
                return new ClassRefactoring(_dte, _solutionParserBridge);

            if (viewModel is FieldViewModel)
                return new FieldRefactoring();

            if (viewModel is MethodViewModel)
                return new MethodRefactoring(_solutionParserBridge);
            
            if (viewModel is NamespaceViewModel)
            {
                var classRefactoring = new ClassRefactoring(_dte, _solutionParserBridge);
                return new NamespaceRefactoring(classRefactoring, _solutionParserBridge);
            }

            if (viewModel is ProjectViewModel)
                return new ProjectRefactoring();

            if (viewModel is PropertyViewModel)
                return new PropertyRefactioring();

            return null;
        }
    }
}
