using System.Windows;
using System.Windows.Input;
using Refactorizer.VSIX.Commands;
using Refactorizer.VSIX.Models;
using Refactorizer.VSIX.Refactorings;
using Task = System.Threading.Tasks.Task;

namespace Refactorizer.VSIX.ViewModels
{
    internal class MethodViewModel : DependencyTreeItemViewModel
    {
        public MethodViewModel(DependencyTreeItemViewModel parent, IModel relatedModel, IRefactoringFactory refactoringFactory) : base(parent, relatedModel, refactoringFactory)
        {
            Delete = new RelayCommand(async param => await DeleteAction(), param => true);
        }

        public ICommand Delete { get; set; }

        private async Task DeleteAction()
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Delete Confirmation", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                var refactoring = Refactoring as MethodRefactoring;
                if (refactoring == null)
                    return;

                var method = RelatedModel as Method;
                if (method == null)
                    return;

                var result = await refactoring.Delete(method);
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

        public override string Name => (RelatedModel as Method)?.Signature ?? RelatedModel.Name;
    }
}
