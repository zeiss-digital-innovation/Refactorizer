using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Refactorizer.VSIX.Analyser;
using Refactorizer.VSIX.Exceptions;
using Refactorizer.VSIX.Models;
using Refactorizer.VSIX.Refactorings;
using Refactorizer.VSIX.ViewModels;

namespace Refactorizer.VSIX.Views
{
    /// <summary>
    /// Interaction logic for SolutionTree.xaml
    /// </summary>
    public partial class SolutionTree : UserControl
    {
        private readonly ICodeAnalyser _codeAnalyser;
        private readonly IRefactoringFactory _refactoringFactory;
        private ISolution _solution;

        public SolutionTree()
        {
            var dte = Package.GetGlobalService(typeof(SDTE)) as DTE;
            _codeAnalyser = new CodeAnalyser(dte);

            _refactoringFactory = new RefactoringFactory(dte);

            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            if (_solution != null)
                return;

            DoCreateGraph();
        }

        private void DoCreateGraph()
        {
            var backgroundWorker = new BackgroundWorker {WorkerReportsProgress = true};
            backgroundWorker.DoWork += CreateGraph;

            ProgressBar.Visibility = Visibility.Visible;
            Error.Visibility = Visibility.Hidden;
            Tree.Visibility = Visibility.Hidden;
            backgroundWorker.RunWorkerAsync();
        }

        private async void CreateGraph(object sender, DoWorkEventArgs e)
        {
            try
            {
                _solution = await _codeAnalyser.GenerateDependencyTree();
                if (_solution != null)
                {
                    Dispatcher.Invoke(() =>
                    {
                        ProgressBar.Visibility = Visibility.Hidden;
                        Tree.Visibility = Visibility.Visible;
                        DataContext = new SolutionViewModel(_solution, _refactoringFactory);
                    });
                }
            }
            catch (NoSolutionOpenException)
            {
                Dispatcher.Invoke(() =>
                {
                    Error.Visibility = Visibility.Visible;
                    ProgressBar.Visibility = Visibility.Hidden;
                    Tree.Visibility = Visibility.Hidden;
                });
            }
        }

        private void Refresh(object sender, RoutedEventArgs e)
        {
            DoCreateGraph();
        }

        private void ClearSelection(object sender, RoutedEventArgs e)
        {
            DependencyTreeControl.ClearSelection();
        }
    }
}
