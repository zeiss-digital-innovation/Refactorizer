using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Refactorizer.VSIX.Models;
using Refactorizer.VSIX.ViewModels;

namespace Refactorizer.VSIX.Controls
{
    internal class DependencyTreeControl : TreeView
    {
        public static readonly DependencyProperty DependencyTreeViewItemsProperty =
            DependencyProperty.Register("DependencyTreeViewItems", typeof(ObservableCollection<DependencyTreeItemControl>), typeof(DependencyTreeControl),
                new UIPropertyMetadata(new ObservableCollection<DependencyTreeItemControl>()));


        public bool SomeChildIsSelected = false;

        public DependencyTreeControl()
        {
        }

        public ObservableCollection<DependencyTreeItemControl> DependencyTreeViewItems
        {
            get => (ObservableCollection<DependencyTreeItemControl>)GetValue(DependencyTreeViewItemsProperty);
            set => SetValue(DependencyTreeViewItemsProperty, value);
        }

        internal AdornerLayer AdornerLayer { get; set; }

        /// <summary>
        /// Init template components
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            AdornerLayer = AdornerLayer.GetAdornerLayer(this);
            DependencyTreeViewItems = new ObservableCollection<DependencyTreeItemControl>();
        }

        /// <summary>
        /// Set the child item type
        /// </summary>
        /// <returns></returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new DependencyTreeItemControl(this);
        }

        public void AddTreeViewItem(DependencyTreeItemControl newItem)
        {
            DependencyTreeViewItems.Add(newItem);

            var viewModel = newItem.DataContext as DependencyTreeItemViewModel;
            if (viewModel == null)
                return;

            var model = viewModel.RelatedModel;
            var rootView = FindParentOfView(model);
            if (rootView == null)
                return;

            var root = FindControlByViewModel(rootView);

            if (root == null)
                return;

            if (root.Childrens.Contains(newItem))
                return;

            newItem.Root = root;
            root.Childrens.Add(newItem);
        }

        public DependencyTreeItemControl FindReferencedItemOrParent(IModel viewModel)
        {
            if (viewModel == null)
                return null;

            var item = FindControlByDataModel(viewModel);
            while (item == null || !item.IsVisible)
            {
                if (viewModel.Parent == null || viewModel is Project)
                    break;

                viewModel = viewModel.Parent;

                var parentControl = FindControlByDataModel(viewModel);

                if (parentControl == null)
                    continue;

                var view = parentControl?.DataContext as DependencyTreeItemViewModel;
                viewModel = view?.RelatedModel;
                item = parentControl;
            }
            return item;
        }

        public List<DependencyTreeItemControl> FindLastExpandedDependencyTreeViewItems(DependencyTreeItemControl root)
        {
            var viewModel = root.DataContext as DependencyTreeItemViewModel;
            if (viewModel == null)
                return null;

            var tmp = new List<DependencyTreeItemControl>();

            foreach (var childViewModel in viewModel.Children)
            {
                var childTreeItem = FindControlByViewModel(childViewModel);
                if (childViewModel.IsExpanded)
                {
                    tmp = tmp.Union(FindLastExpandedDependencyTreeViewItems(childTreeItem)).ToList();
                }
                else
                {
                    if (!tmp.Contains(childTreeItem))
                        tmp.Add(childTreeItem);
                }
            }

            return tmp;
        }

        public DependencyTreeItemControl FindControlByViewModel(DependencyTreeItemViewModel itemViewModelModel)
        {
            foreach (var viewItem in DependencyTreeViewItems)
            {
                var dataModel = viewItem.DataContext as DependencyTreeItemViewModel;
                if (dataModel == null)
                    continue;

                if (dataModel.RelatedModel.Id.Equals(itemViewModelModel.RelatedModel.Id))
                    return viewItem;
            }

            return null;
        }

        public DependencyTreeItemControl FindControlByDataModel(IModel dataModel)
        {
            foreach (var dependencyTreeViewItem in DependencyTreeViewItems)
            {
                var itemViewModel = dependencyTreeViewItem.DataContext as DependencyTreeItemViewModel;
                if (itemViewModel != null)
                {
                    if (itemViewModel.RelatedModel.Id.Equals(dataModel.Id))
                    {
                        return dependencyTreeViewItem;
                    }
                }
            }

            return null;
        }

        public DependencyTreeItemViewModel FindParentOfView(IModel child)
        {
            var solutionViewModel = DataContext as SolutionViewModel;
            if (solutionViewModel == null)
                return null;
            var index = 0;
        
            while (index < solutionViewModel.Projects.Count)
            {
                var projectViewModel = solutionViewModel.Projects.ToList()[index];
                var relatedModel = projectViewModel.RelatedModel;
                if (relatedModel.Id.Equals(child.Id))
                {
                    return null;
                }
                var result = FindParentOfView(projectViewModel, child);
                if (result != null)
                    return result;

                index++;
            }

            return null;
        }

        private DependencyTreeItemViewModel FindParentOfView(DependencyTreeItemViewModel parent, IModel child)
        {
            var index = 0;
            while (index < parent.Children.Count)
            {
                var dependencyTreeItemViewModel = parent.Children.ToList()[index];
                if (dependencyTreeItemViewModel.RelatedModel.Id.Equals(child.Id))
                    return parent;

                var result = FindParentOfView(dependencyTreeItemViewModel, child);
                if (result != null)
                    return result;

                index++;
            }

            return null;
        }

        public bool Contains(DependencyTreeItemControl dependencyTreeItemControl)
        {
            var dependencyTreeItemView = dependencyTreeItemControl.DataContext as DependencyTreeItemViewModel;
            if (dependencyTreeItemView == null)
                return false;

            var id = dependencyTreeItemView.RelatedModel.Id;
            return DependencyTreeViewItems.Select(x => x.DataContext as DependencyTreeItemViewModel).ToList()
                .Where(x => x != null).ToList()
                .Where(x => x.RelatedModel != null).ToList()
                .Exists(x => x.RelatedModel.Id.Equals(id));
        }

        public void ClearSelection()
        {
            var selected = DependencyTreeViewItems.Where(x => x.IsSelected = true).ToList();
            foreach (var control in selected)
            {
                control.IsSelected = false;
            }
        }
    }
}
