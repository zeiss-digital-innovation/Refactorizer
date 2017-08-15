using System;
using System.Windows;
using System.Windows.Input;
using Refactorizer.VSIX.Commands;

namespace Refactorizer.VSIX.ViewModels
{
    internal class RenameDialogViewModel : DependencyObject
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(RenameDialogViewModel));

        public string Text
        {
            get => (string) GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public ICommand Confirm { get; set; }

        public ICommand Cancel { get; set; }

        public RenameDialogViewModel(Action confirmAction, Action cancelAction, string text = "")
        {
            Confirm = new RelayCommand(param => confirmAction());
            Cancel = new RelayCommand(param => cancelAction());
            Text = text;
        }
    }
}