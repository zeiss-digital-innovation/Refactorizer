using EnvDTE;
using System.Windows.Input;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Refactorizer.VSIX.Models;

namespace Refactorizer.VSIX.View
{
    class ClassItemView : DependencyTreeItemView
    {

        public ClassItemView(DependencyTreeItemView parent, IModel relatedModel) : base(parent, relatedModel)
        {
            Open = new RelayCommand(param => OpenFile(), param => CanOpen());

            var @class = RelatedModel as Class;
            if (@class == null)
                return;

            foreach (var field in @class.Fields)
                Children.Add(new FieldItemView(this, field));
            
            foreach (var property in @class.Properties)
                Children.Add(new PropertyItemView(this, property));

            foreach (var method in @class.Methods)
                Children.Add(new MethodItemView(this, method));
        }

        public ICommand Open { get; set; }

        private bool CanOpen()
        {
            return true;
        }

        private void OpenFile()
        {
            var @class = RelatedModel as Class;
            if (@class == null)
                return;

            //var dte = Package.GetGlobalService(typeof(SDTE)) as DTE;
            //dte?.MainWindow.Activate();
            //dte?.ItemOperations.OpenFile(@class.Path, EnvDTE.Constants.vsViewKindTextView);
            // ((EnvDTE.TextSelection)dte2.ActiveDocument.Selection).GotoLine(fileline, true);
        }
    }
}