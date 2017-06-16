using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Refactorizer.VSIX.ViewModels
{
    public static class TreeViewItemBehavior
    {
        public static bool GetCoordinate(TreeViewItem treeViewItem)
        {
            return (bool) treeViewItem.GetValue(Coordinate);
        }

        public static void SetCoordinate(TreeViewItem treeViewItem, bool value)
        {
            treeViewItem.SetValue(Coordinate, value);
        }

        public static readonly DependencyProperty Coordinate =
            DependencyProperty.RegisterAttached(
                "Coordinate",
                typeof(bool),
                typeof(TreeViewItemBehavior), 
                new UIPropertyMetadata(false, OnIsCoodinateChanged));

        private static void OnIsCoodinateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TreeViewItem item = d as TreeViewItem;
            if (item == null)
                return;

            if (e.NewValue is bool == false)
                return;

            if ((bool)e.NewValue)
            {
                item.Loaded += OnTreeViewItemIsInitialized;
            } else {
                item.Loaded -= OnTreeViewItemIsInitialized;
            }
        }

        private static void OnTreeViewItemIsInitialized(object sender, RoutedEventArgs e)
        {
            // Only react to the Selected event raised by the TreeViewItem
            // whose IsSelected property was modified.  Ignore all ancestors
            // who are merely reporting that a descendant's Selected fired.
            if (!Object.ReferenceEquals(sender, e.OriginalSource))
                return;

            TreeViewItem item = e.OriginalSource as TreeViewItem;
            var point = item.PointToScreen(new Point(0, 0));
            var treeViewItemViewModel = item.DataContext as TreeViewItemViewModel;
            if (treeViewItemViewModel != null)
                treeViewItemViewModel.Coordinate = point;
        }
    }
}
