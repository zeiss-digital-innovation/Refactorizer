using System.Windows;
using System.Windows.Input;
using Refactorizer.VSIX.Commands;
using Refactorizer.VSIX.Misc;
using Refactorizer.VSIX.Models;
using Refactorizer.VSIX.Refactorings;
using Refactorizer.VSIX.Views;
using Task = System.Threading.Tasks.Task;

namespace Refactorizer.VSIX.ViewModels
{
    internal class ClassViewModel : DependencyTreeItemViewModel
    {
        public ICommand Open { get; set; }

        public ICommand Delete { get; set; }
        
        public ICommand Rename { get; set; }

        public ClassViewModel(SolutionViewModel root, DependencyTreeItemViewModel parent, IModel relatedModel, 
            IRefactoringFactory refactoringFactory) : base(root, parent, relatedModel, refactoringFactory)
        {
            Open = new RelayCommand(async param => await OpenAction());
            Delete = new RelayCommand(param => DeleteAction());
            Rename = new RelayCommand(param => RenameAction());

            var @class = RelatedModel as Class;
            if (@class == null)
                return;

            foreach (var field in @class.Fields)
                Children.Add(new FieldViewModel(root, this, field, refactoringFactory));
            
            foreach (var property in @class.Properties)
                Children.Add(new PropertyViewModel(root, this, property, refactoringFactory));

            foreach (var method in @class.Methods)
                Children.Add(new MethodViewModel(root, this, method, refactoringFactory));
        }

        private async Task OpenAction()
        {
            var refactoring = Refactoring as ClassRefactoring;
            if (refactoring == null)
                return;

            var @class = RelatedModel as Class;
            if (@class == null)
                return;

            await refactoring.Open(@class);
        }

        private void DeleteAction()
        {
            DialogManager.Create("Confirm delete", new DeleteConfirmDialog(async () =>
            {
                var refactoring = Refactoring as ClassRefactoring;
                if (refactoring == null)
                    return;

                var @class = RelatedModel as Class;
                if (@class == null)
                    return;

                var result = await refactoring.Delete(@class);
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
            var refactoring = Refactoring as ClassRefactoring;
            if (refactoring == null)
                return;

            var @class = RelatedModel as Class;
            if (@class == null)
                return;

            var control = new RenameDialog(async () =>
            {
                var content = DialogManager.GetContent();
                var viewModel = content.DataContext as RenameDialogViewModel;
                if (viewModel == null)
                    return;

                var newName = viewModel.Text;
                await refactoring.Rename(@class, newName);
                Name = newName;

                DialogManager.Close();
            }, DialogManager.Close, @class.Name);

            DialogManager.Create("Rename", control);
        }
    }
}