using System.Collections.ObjectModel;
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
                // Load all childs 
                RemoveDummyAndLoadChildren();

                // Expand all parents
                if (_parent != null && !_parent.IsExpanded)
                    _parent.IsSelected = true;

                _isExpanded = value;
                SetField(ref _isExpanded, value, "IsExpanded");
            }
        }

        public void RemoveDummyAndLoadChildren()
        {
            if (HasDummyChild)
            {
                Children.Remove(DummyChild);
                Loadchildren();
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

        public bool HasDummyChild => this.Children.Count == 1 && this.Children[0] == DummyChild;

        protected DependencyTreeItemView(DependencyTreeItemView parent, IModel relatedModel)
        {
            _parent = parent;
            RelatedModel = relatedModel;

            if (RelatedModel.HasChildren)
                AddDummy();
        }

        private DependencyTreeItemView()
        {
        }

        protected void AddDummy()
        {
            this.Children.Add(DummyChild);
        }

        public virtual void Loadchildren()
        {
        }

        
        public virtual string Name => RelatedModel.Name;
    }
}
