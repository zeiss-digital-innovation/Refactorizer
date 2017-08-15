using System.Collections.ObjectModel;
using Refactorizer.VSIX.Misc;
using Refactorizer.VSIX.Models;
using Refactorizer.VSIX.Refactorings;

namespace Refactorizer.VSIX.ViewModels
{
    internal abstract class DependencyTreeItemViewModel : NotifyPropertyChanged, IDependencyTreeItemViewModel
    {
        protected readonly DependencyTreeItemViewModel Parent;

        protected readonly SolutionViewModel Root;

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

        protected DependencyTreeItemViewModel(SolutionViewModel root, DependencyTreeItemViewModel parent, IModel relatedModel, IRefactoringFactory refactoringFactory)
        {
            Parent = parent;
            RelatedModel = relatedModel;
            Root = root;

            Refactoring = refactoringFactory.GetRefactoringForViewModel(this);

        }

        protected internal void DeleteChild(DependencyTreeItemViewModel child)
        {
            Children.Remove(child);
        }

        protected string RelatedModelName;

        public virtual string Name
        {
            get => RelatedModel.Name;
            set { 
                RelatedModel.Name = value;
                SetField(ref RelatedModelName, value, "Name");
            }
        }
    }
}
