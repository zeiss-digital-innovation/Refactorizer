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
    internal class DependencyTreeView : TreeView
    {
        public static readonly DependencyProperty DependencyTreeViewItemsProperty =
            DependencyProperty.Register("DependencyTreeViewItems", typeof(ObservableCollection<DependencyTreeViewItem>), typeof(DependencyTreeView),
                new UIPropertyMetadata(new ObservableCollection<DependencyTreeViewItem>()));

        private ObservableCollection<DependencyTreeViewItem> DependencyTreeViewItems
        {
            get => (ObservableCollection<DependencyTreeViewItem>)GetValue(DependencyTreeViewItemsProperty);
            set => SetValue(DependencyTreeViewItemsProperty, value);
        }

        internal AdornerLayer OutReferencesAdornerLayer { get; set; }

        /// <summary>
        /// Init template components
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            OutReferencesAdornerLayer = AdornerLayer.GetAdornerLayer(this);
            DependencyTreeViewItems = new ObservableCollection<DependencyTreeViewItem>();
        }

        /// <summary>
        /// Set the child item type
        /// </summary>
        /// <returns></returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new DependencyTreeViewItem(this);
        }

        public void AddTreeViewItem(DependencyTreeViewItem newItem)
        {
            DependencyTreeViewItems.Add(newItem);

            // Search root
            var view = newItem.DataContext as 
        }

        public DependencyTreeViewItem FindReferencedItemOrParent(IModel viewModel)
        {
            var item = FindViewModelByDataModel(viewModel);

            // If not found search traversal for namespaces and classes
            if ((item == null || !item.IsVisible) && (viewModel is Namespace || viewModel is Class || viewModel is Method || viewModel is Property || viewModel is Field))
            {
                var parentTreeViewItem = FindViewModelByDataModel(viewModel.Parent);
                var reference = parentTreeViewItem?.DataContext as DependencyTreeItemView;
                if (reference != null)
                {
                    item = FindReferencedItemOrParent(reference.RelatedModel);
                }
            }

            return item;
        }

        public List<DependencyTreeViewItem> FindLastExpandedDependencyTreeViewItems(DependencyTreeViewItem root)
        {
            var viewModel = root.DataContext as DependencyTreeItemView;
            if (viewModel == null)
                return null;

            var tmp = new List<DependencyTreeViewItem>();

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

        public DependencyTreeViewItem FindViewItemByViewModel(DependencyTreeItemView itemViewModel)
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

        public DependencyTreeViewItem FindViewModelByDataModel(IModel dataModel)
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

        public bool Contains(DependencyTreeViewItem dependencyTreeViewItem)
        {
            return DependencyTreeViewItems.Contains(dependencyTreeViewItem);
        }
    }
}
