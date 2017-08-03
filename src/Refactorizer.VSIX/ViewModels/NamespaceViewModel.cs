using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Refactorizer.VSIX.Commands;
using Refactorizer.VSIX.Models;
using Refactorizer.VSIX.Refactorings;

namespace Refactorizer.VSIX.ViewModels
{
    internal class NamespaceViewModel : DependencyTreeItemViewModel
    {
        public ICommand Delete { get; set; }

        public NamespaceViewModel(DependencyTreeItemViewModel parent, IModel relatedModel, IRefactoringFactory refactoringFactory) : base(parent, relatedModel, refactoringFactory)
        {
            Delete = new RelayCommand(async param => await DeleteAction(), param => true);

            var @namespace = RelatedModel as Namespace;
            if (@namespace == null)
                return;

            foreach (var @class in @namespace.Classes)
                Children.Add(new ClassViewModel(this, @class, refactoringFactory));
        }

        private async Task DeleteAction()
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Delete Confirmation", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
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
            }
        }
    }
}