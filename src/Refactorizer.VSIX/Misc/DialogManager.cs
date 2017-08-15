using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.PlatformUI;

namespace Refactorizer.VSIX.Misc
{
    class DialogManager
    {
        private static DialogWindow _window;

        private static UserControl _content;

        public static DialogWindow Create(string title, UserControl content)
        {
            // Allow only one dialog used
            if (_window != null)
                Close();

            _content = content;
            _window = new DialogWindow
            {
                Content = content,
                Title = title,
                SizeToContent = SizeToContent.WidthAndHeight
            };
            _window.Show();

            return _window;
        }

        public static void Close()
        {
            _window.Close();
        }

        public static DialogWindow GetWindow()
        {
            return _window;
        }

        public static UserControl GetContent()
        {
            return _content;
        }
    }
}
