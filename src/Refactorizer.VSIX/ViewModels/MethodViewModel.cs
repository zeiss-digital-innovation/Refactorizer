using System.Windows;
using System.Windows.Input;
using Refactorizer.VSIX.Commands;
using Refactorizer.VSIX.Misc;
using Refactorizer.VSIX.Models;
using Refactorizer.VSIX.Refactorings;
using Refactorizer.VSIX.Views;

namespace Refactorizer.VSIX.ViewModels
{
    internal class MethodViewModel : DependencyTreeItemViewModel
    {
        public MethodViewModel(SolutionViewModel root, DependencyTreeItemViewModel parent, IModel relatedModel,
            IRefactoringFactory refactoringFactory) : base(root, parent, relatedModel, refactoringFactory)
        {
            Delete = new RelayCommand(param => DeleteAction());
            Rename = new RelayCommand(param => RenameAction());
        }

        public ICommand Delete { get; set; }

        public ICommand Rename { get; set; }

        private void DeleteAction()
        {
            var refactoring = Refactoring as MethodRefactoring;
            if (refactoring == null)
                return;

            var method = RelatedModel as Method;
            if (method == null)
                return;

            DialogManager.Create("Confirm delete", new DeleteConfirmDialog(async () =>
            {
                var result = await refactoring.Delete(method);
                if (result == ActionResult.Success)
                {
                    Parent.DeleteChild(this);
                }
                else
                {
                    MessageBox.Show("Delete failed");
                }
                DialogManager.Close();
            }, DialogManager.Close));
        }

        private void RenameAction()
        {
            var refactoring = Refactoring as MethodRefactoring;
            if (refactoring == null)
                return;

            var method = RelatedModel as Method;
            if (method == null)
                return;

            DialogManager.Create("Rename", new RenameDialog(async () =>
            {
                var content = DialogManager.GetContent();
                var viewModel = content.DataContext as RenameDialogViewModel;
                if (viewModel == null)
                    return;

                var newName = viewModel.Text;
                await refactoring.Rename(method, newName);

                Name = newName;
                DialogManager.Close();
            }, DialogManager.Close, method.Name));
        }

        public override string Name
        {
            get => (RelatedModel as Method)?.Signature ?? RelatedModel.Name;
            set { 
                RelatedModel.Name = value;
                SetField(ref RelatedModelName, value, "Name");
            }
        }
    }
}