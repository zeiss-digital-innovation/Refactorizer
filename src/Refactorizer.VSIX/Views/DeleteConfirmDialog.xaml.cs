using System;
using System.Windows.Controls;
using System.Windows.Input;
using Refactorizer.VSIX.Commands;
using Refactorizer.VSIX.ViewModels;

namespace Refactorizer.VSIX.Views
{
    /// <summary>
    /// Interaction logic for DeleteConfirmDialog.xaml
    /// </summary>
    public partial class DeleteConfirmDialog : UserControl
    {
        public DeleteConfirmDialog(Action confirmAction, Action cancelAction)
        {
            InitializeComponent();

            DataContext = new ConfirmDialogViewModel(confirmAction, cancelAction)
            {
                Message = "Are you sure to delete this item?"
            };
        }
    }
}
