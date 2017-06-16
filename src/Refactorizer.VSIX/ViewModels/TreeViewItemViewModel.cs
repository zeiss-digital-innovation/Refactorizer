using System.Collections.ObjectModel;
using System.Windows;

namespace Refactorizer.VSIX.ViewModels
{
    public class TreeViewItemViewModel : NotifyPropertyChanged
    {
        private readonly TreeViewItemViewModel _parent;

        private static readonly TreeViewItemViewModel DummyChild = new TreeViewItemViewModel();

        private bool _isExpanded;
        private bool _isSelected;

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                // Load all childs 
                if (HasDummyChild)
                {
                    Children.Remove(DummyChild);
                    Loadchildren();
                }

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

        private Point _coordinate;

        public Point Coordinate
        {
            get => _coordinate;
            set
            {
                _coordinate = value;
                SetField(ref _coordinate, value, "Coordinate");
            }
        }

        public ObservableCollection<TreeViewItemViewModel> Children { get; } = new ObservableCollection<TreeViewItemViewModel>();

        public bool HasDummyChild => this.Children.Count == 1 && this.Children[0] == DummyChild;

        protected TreeViewItemViewModel(TreeViewItemViewModel parent)
        {
            _parent = parent;
        }

        public TreeViewItemViewModel()
        {
        }

        protected void AddDummy()
        {
            this.Children.Add(DummyChild);
        }

        protected virtual void Loadchildren()
        {
        }

        public virtual string Name => "Dummy";
    }
}
