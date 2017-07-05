using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using Refactorizer.VSIX.ViewModels;

namespace Refactorizer.VSIX.CustomControl
{
    class DepenendcyTreeViewItem : TreeViewItem
    {
        private readonly DependencyTreeView _host;

        public ObservableCollection<TreeLineAdorner> OutLines { get; set; }

        public ObservableCollection<TreeLineAdorner> IntLines { get; set; }

        public DepenendcyTreeViewItem(DependencyTreeView host)
        {
            this._host = host;
            this.DataContextChanged += TreeCanvasItemDataContextChanged;
            OutLines = new ObservableCollection<TreeLineAdorner>();
            InLines = new ObservableCollection<TreeLineAdorner>();
        }

        /// <summary>
        /// Set the child item type
        /// </summary>
        /// <returns></returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new DepenendcyTreeViewItem(_host);
        }

        private void TreeCanvasItemDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _host.DependencyTreeViewItems.Add(this);
        }
    }
}
