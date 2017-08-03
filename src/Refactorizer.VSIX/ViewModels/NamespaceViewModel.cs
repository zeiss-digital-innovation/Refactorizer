using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Refactorizer.VSIX.Commands;
using Refactorizer.VSIX.Misc;
using Refactorizer.VSIX.Models;
using Refactorizer.VSIX.Refactorings;
using Refactorizer.VSIX.Views;

namespace Refactorizer.VSIX.ViewModels
{
    internal class NamespaceViewModel : DependencyTreeItemViewModel
    {
        public ICommand Delete { get; set; }

        public NamespaceViewModel(DependencyTreeItemViewModel parent, IModel relatedModel, IRefactoringFactory refactoringFactory) : base(parent, relatedModel, refactoringFactory)
        {
            Delete = new RelayCommand(param => DeleteAction(), param => true);

            var @namespace = RelatedModel as Namespace;
            if (@namespace == null)
                return;

            foreach (var @class in @namespace.Classes)
                Children.Add(new ClassViewModel(this, @class, refactoringFactory));
        }

        private void DeleteAction()
        {
            DialogManager.Create("Confirm delete", new DeleteConfirmDialog(async () =>
            {
                var refactoring = Refactoring as NamespaceRefactoring;
                if (refactoring == null)
                    return;

                var ns = RelatedModel as Namespace;
                if (ns == null)
                    return;

                var result = await refactoring.Delete(ns);
                if (result == ActionResult.Success)
                {
                    Parent.DeleteChild(this);
                }
                else
                {
                    MessageBox.Show("Delete failed");
                }
            }, DialogManager.Close));
        }
    }
}