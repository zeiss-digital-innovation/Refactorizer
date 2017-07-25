using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Refactorizer.VSIX.Models;
using Refactorizer.VSIX.View;

namespace Refactorizer.VSIX.Controls
{
    internal class DependencyTreeControl : TreeView
    {
        public static readonly DependencyProperty DependencyTreeViewItemsProperty =
            DependencyProperty.Register("DependencyTreeViewItems", typeof(ObservableCollection<DependencyTreeItemControl>), typeof(DependencyTreeControl),
                new UIPropertyMetadata(new ObservableCollection<DependencyTreeItemControl>()));

        private ObservableCollection<DependencyTreeItemControl> DependencyTreeViewItems
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

            var view = newItem.DataContext as DependencyTreeItemView;
            if (view == null)
                throw new System.Exception($"DataContext is not a DependencyTreeItemView");

            // Search root
            var root = DependencyTreeViewItems.FirstOrDefault(
                x =>
                {
                    var dependencyTreeItemView = x.DataContext as DependencyTreeItemView;
                    return dependencyTreeItemView != null &&
                           dependencyTreeItemView.RelatedModel.Id.Equals(view.RelatedModel.Id);
                });

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

                var view = parentControl?.DataContext as DependencyTreeItemView;
                viewModel = view?.RelatedModel;
                item = parentControl;
            }
            return item;
        }

        public List<DependencyTreeItemControl> FindLastExpandedDependencyTreeViewItems(DependencyTreeItemControl root)
        {
            var viewModel = root.DataContext as DependencyTreeItemView;
            if (viewModel == null)
                return null;

            var tmp = new List<DependencyTreeItemControl>();

            foreach (var childViewModel in viewModel.Children)
            {
                var childTreeItem = FindViewItemByViewModel(childViewModel);
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

        public DependencyTreeItemControl FindViewItemByViewModel(DependencyTreeItemView itemViewModel)
        {
            foreach (var viewItem in DependencyTreeViewItems)
            {
                var dataModel = viewItem.DataContext as DependencyTreeItemView;
                if (dataModel == null)
                    continue;

                if (dataModel.RelatedModel.Id.Equals(itemViewModel.RelatedModel.Id))
                    return viewItem;
            }

            return null;
        }

        public DependencyTreeItemControl FindControlByDataModel(IModel dataModel)
        {
            foreach (var dependencyTreeViewItem in DependencyTreeViewItems)
            {
                var itemViewModel = dependencyTreeViewItem.DataContext as DependencyTreeItemView;
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

        public bool Contains(DependencyTreeItemControl dependencyTreeItemControl)
        {
            var dependencyTreeItemView = dependencyTreeItemControl.DataContext as DependencyTreeItemView;
            if (dependencyTreeItemView == null)
                return false;

            var id = dependencyTreeItemView.RelatedModel.Id;
            return DependencyTreeViewItems.Select(x => x.DataContext as DependencyTreeItemView).ToList()
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
