using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Refactorizer.VSIX.Models;

namespace Refactorizer.VSIX.ViewModels
{
    public class DependencyTreeViewItemViewModel : NotifyPropertyChanged
    {
        private readonly DependencyTreeViewItemViewModel _parent;

        public IModel RelatedModel { get; }

        private static readonly DependencyTreeViewItemViewModel DummyChild = new DependencyTreeViewItemViewModel();

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

        public ObservableCollection<DependencyTreeViewItemViewModel> Children { get; } = new ObservableCollection<DependencyTreeViewItemViewModel>();

        // public ObservableCollection<DependencyTreeViewItemViewModel> References { get; } = new ObservableCollection<DependencyTreeViewItemViewModel>();

        public bool HasDummyChild => this.Children.Count == 1 && this.Children[0] == DummyChild;

        protected DependencyTreeViewItemViewModel(DependencyTreeViewItemViewModel parent, IModel relatedModel)
        {
            _parent = parent;
            RelatedModel = relatedModel;
        }

        private DependencyTreeViewItemViewModel()
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
