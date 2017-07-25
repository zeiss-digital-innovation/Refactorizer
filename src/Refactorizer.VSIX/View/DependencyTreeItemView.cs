using System.Collections.ObjectModel;
using System.Windows.Input;
using Refactorizer.VSIX.Models;

namespace Refactorizer.VSIX.View
{
    public class DependencyTreeItemView : NotifyPropertyChanged
    {
        private readonly DependencyTreeItemView _parent;

        public IModel RelatedModel { get; }

        private static readonly DependencyTreeItemView DummyChild = new DependencyTreeItemView();

        private bool _isExpanded;
        private bool _isSelected;

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                // Expand all parents
                if (_parent != null && !_parent.IsExpanded)
                    _parent.IsSelected = true;

                _isExpanded = value;
                SetField(ref _isExpanded, value, "IsExpanded");
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                SetField(ref _isSelected, value, "IsSelected");
            }
        }

        public ObservableCollection<DependencyTreeItemView> Children { get; } = new ObservableCollection<DependencyTreeItemView>();

        // public ObservableCollection<DependencyTreeViewItemViewModel> References { get; } = new ObservableCollection<DependencyTreeViewItemViewModel>();

        protected DependencyTreeItemView(DependencyTreeItemView parent, IModel relatedModel)
        {
            _parent = parent;
            RelatedModel = relatedModel;
        }

        private DependencyTreeItemView()
        {
        }

        public virtual string Name => RelatedModel.Name;
    }
}
