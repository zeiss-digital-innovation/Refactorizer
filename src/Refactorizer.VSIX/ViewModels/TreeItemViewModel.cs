using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Refactorizer.VSIX.Models;

namespace Refactorizer.VSIX.ViewModels
{
    public class TreeItemViewModel : NotifyPropertyChanged
    {
        private readonly TreeItemViewModel _parent;

        public IModel RelatedModel { get; }

        private static readonly TreeItemViewModel DummyChild = new TreeItemViewModel();

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

        public ObservableCollection<TreeItemViewModel> Children { get; } = new ObservableCollection<TreeItemViewModel>();

        public ObservableCollection<TreeItemViewModel> References { get; } = new ObservableCollection<TreeItemViewModel>();

        public bool HasDummyChild => this.Children.Count == 1 && this.Children[0] == DummyChild;

        protected TreeItemViewModel(TreeItemViewModel parent, IModel relatedModel)
        {
            _parent = parent;
            RelatedModel = relatedModel;
        }

        private TreeItemViewModel()
        {
        }

        protected void AddDummy()
        {
            this.Children.Add(DummyChild);
        }

        protected virtual void Loadchildren()
        {
        }

        public virtual string Name => RelatedModel.Name;
    }
}
