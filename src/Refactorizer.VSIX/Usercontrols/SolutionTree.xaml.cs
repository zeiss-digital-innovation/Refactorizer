using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Refactorizer.VSIX.Exception;
using Refactorizer.VSIX.Models;
using Refactorizer.VSIX.ViewModels;

namespace Refactorizer.VSIX.Usercontrols
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
            var backgroundWorker = new BackgroundWorker {WorkerReportsProgress = true};
            backgroundWorker.DoWork += CreateGraph;

            ProgressBar.Visibility = Visibility.Visible;
            Error.Visibility = Visibility.Hidden;
            backgroundWorker.RunWorkerAsync();
            Solution.TransformToAncestor(this).Transform(new Point(0, 0));
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
                });
            }
        }
    }
}
