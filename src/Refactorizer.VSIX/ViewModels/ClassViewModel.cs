using System.Windows;
using System.Windows.Input;
using Refactorizer.VSIX.Commands;
using Refactorizer.VSIX.Models;
using Refactorizer.VSIX.Refactorings;
using Task = System.Threading.Tasks.Task;

namespace Refactorizer.VSIX.ViewModels
{
    internal class ClassViewModel : DependencyTreeItemViewModel
    {
        public ICommand Open { get; set; }

        public ICommand Delete { get; set; }

        public ClassViewModel(DependencyTreeItemViewModel parent, IModel relatedModel, IRefactoringFactory refactoringFactory) : base(parent, relatedModel, refactoringFactory)
        {
            Open = new RelayCommand(async param => await OpenAction(), param => true);
            Delete = new RelayCommand(async param => await DeleteAction(), param => true);

            var @class = RelatedModel as Class;
            if (@class == null)
                return;

            foreach (var field in @class.Fields)
                Children.Add(new FieldViewModel(this, field, refactoringFactory));
            
            foreach (var property in @class.Properties)
                Children.Add(new PropertyViewModel(this, property, refactoringFactory));

            foreach (var method in @class.Methods)
                Children.Add(new MethodViewModel(this, method, refactoringFactory));
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

        private async Task DeleteAction()
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Delete Confirmation", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
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
            }
        }
    }
}