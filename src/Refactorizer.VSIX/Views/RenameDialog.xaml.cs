using System;
using System.Windows.Controls;
using Refactorizer.VSIX.ViewModels;

namespace Refactorizer.VSIX.Views
{
    /// <summary>
    ///     Interaction logic for RenameDialog.xaml
    /// </summary>
    public partial class RenameDialog : UserControl
    {
        public RenameDialog(Action confirmAction, Action cancelAction, string title = "")
        {
            InitializeComponent();

            DataContext = new RenameDialogViewModel(confirmAction, cancelAction, title);
        }
    }
}