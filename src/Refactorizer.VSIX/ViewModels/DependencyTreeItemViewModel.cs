using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Refactorizer.VSIX.Commands;
using Refactorizer.VSIX.Misc;
using Refactorizer.VSIX.Models;
using Refactorizer.VSIX.Refactoring;

namespace Refactorizer.VSIX.ViewModels
{
    internal abstract class DependencyTreeItemViewModel : NotifyPropertyChanged, IDependencyTreeItemViewModel
    {
        protected readonly DependencyTreeItemViewModel Parent;

        public IModel RelatedModel { get; }

        protected IRefactoring Refactoring;

        private bool _isExpanded;

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                // Expand all parents
                if (Parent != null && !Parent.IsExpanded)
                    Parent.IsSelected = true;

                _isExpanded = value;
                SetField(ref _isExpanded, value, "IsExpanded");
            }
        }

        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                SetField(ref _isSelected, value, "IsSelected");
            }
        }

        public ObservableCollection<DependencyTreeItemViewModel> Children { get; } = new ObservableCollection<DependencyTreeItemViewModel>();

        protected DependencyTreeItemViewModel(DependencyTreeItemViewModel parent, IModel relatedModel, IRefactoringFactory refactoringFactory)
        {
            Parent = parent;
            RelatedModel = relatedModel;

            Refactoring = refactoringFactory.GetRefactoringForViewModel(this);

        }

        protected internal void DeleteChild(DependencyTreeItemViewModel child)
        {
            Children.Remove(child);
        }

        public virtual string Name => RelatedModel.Name;
    }
}
