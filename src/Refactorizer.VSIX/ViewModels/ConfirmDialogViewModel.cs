using System;
using System.Windows;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using Refactorizer.VSIX.Commands;

namespace Refactorizer.VSIX.ViewModels
{
    public class ConfirmDialogViewModel
    {
        public string Message { get; set; }

        public ICommand Confirm { get; set; }

        public ICommand Cancel { get; set; }

        public ConfirmDialogViewModel(Action confirmAction, Action cancelAction)
        {
            Confirm = new RelayCommand(param => confirmAction());
            Cancel = new RelayCommand(param => cancelAction());
        }
    }
}