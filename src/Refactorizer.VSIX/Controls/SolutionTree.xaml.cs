using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Refactorizer.VSIX.Exception;
using Refactorizer.VSIX.Models;
using Refactorizer.VSIX.View;

namespace Refactorizer.VSIX.Controls
{
    /// <summary>
    /// Interaction logic for SolutionTree.xaml
    /// </summary>
    public partial class SolutionTree : UserControl
    {
        private readonly CodeAnalyser _codeAnalyser;
        private Solution _solution;

        public SolutionTree()
        {
            InitializeComponent();
            
            _codeAnalyser = new CodeAnalyser();
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
                        DataContext = new SolutionViewModel(_solution);
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
