using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.PlatformUI;

namespace Refactorizer.VSIX.Misc
{
    class DialogManager
    {
        private static DialogWindow _dialogWindow;

        public static DialogWindow Create(string title, UserControl content)
        {
            _dialogWindow = new DialogWindow
            {
                Content = content,
                Title = title,
                SizeToContent = SizeToContent.WidthAndHeight
            };
            _dialogWindow.Show();

            return _dialogWindow;
        }

        public static void Close()
        {
            _dialogWindow.Close();
        }
    }
}
