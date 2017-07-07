using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Refactorizer.VSIX.Models;
using Refactorizer.VSIX.ViewModels;

namespace Refactorizer.VSIX.CustomControl
{
    internal class DependencyTreeView : TreeView
    {
        public static readonly DependencyProperty DependencyTreeViewItemsProperty =
            DependencyProperty.Register("DependencyTreeViewItems", typeof(ObservableCollection<DependencyTreeViewItem>), typeof(DependencyTreeView),
                new UIPropertyMetadata(new ObservableCollection<DependencyTreeViewItem>()));

        public ObservableCollection<DependencyTreeViewItem> DependencyTreeViewItems
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

        public DependencyTreeViewItem FindReferencedItemOrParent(IModel viewModel)
        {
            var item = FindViewModelByDataModel(viewModel);

            // If not found search traversal for namespaces and classes
            if ((item == null || !item.IsVisible) && (viewModel is Namespace || viewModel is Class || viewModel is Method))
            {
                var parentTreeViewItem = FindViewModelByDataModel(viewModel.Parent);
                var reference = parentTreeViewItem?.DataContext as DependencyTreeViewItemViewModel;
                if (reference != null)
                {
                    item = FindReferencedItemOrParent(reference.RelatedModel);
                }
            }

            return item;
        }

        public List<DependencyTreeViewItem> FindLastExpandedDependencyTreeViewItems(DependencyTreeViewItem root)
        {
            var viewModel = root.DataContext as DependencyTreeViewItemViewModel;
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

        public DependencyTreeViewItem FindViewItemByViewModel(DependencyTreeViewItemViewModel viewModel)
        {
            foreach (var viewItem in DependencyTreeViewItems)
            {
                var dataModel = viewItem.DataContext as DependencyTreeViewItemViewModel;
                if (dataModel == null)
                    continue;

                if (dataModel.RelatedModel.Id.Equals(viewModel.RelatedModel.Id))
                    return viewItem;
            }

            return null;
        }

        public DependencyTreeViewItem FindViewModelByDataModel(IModel dataModel)
        {
            foreach (var dependencyTreeViewItem in DependencyTreeViewItems)
            {
                var itemViewModel = dependencyTreeViewItem.DataContext as DependencyTreeViewItemViewModel;
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
    }
}
