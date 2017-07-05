using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Refactorizer.VSIX.ViewModels;

namespace Refactorizer.VSIX.CustomControl
{
    internal class DependencyTreeViewItem : TreeViewItem
    {
        private readonly DependencyTreeView _host;

        private readonly Dictionary<Guid, BezierCurveAdorner> _drawedLines = new Dictionary<Guid, BezierCurveAdorner>();

        public DependencyTreeViewItem(DependencyTreeView host)
        {
            _host = host;
            DataContextChanged += TreeCanvasItemDataContextChanged;
        }

        public ObservableCollection<BezierCurveAdorner> OutLines { get; set; } =
            new ObservableCollection<BezierCurveAdorner>();

        public ObservableCollection<BezierCurveAdorner> InLines { get; set; } =
            new ObservableCollection<BezierCurveAdorner>();

        /// <summary>
        ///     Set the child item type
        /// </summary>
        /// <returns></returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new DependencyTreeViewItem(_host);
        }

        private void TreeCanvasItemDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!_host.DependencyTreeViewItems.Contains(this))
            {
                _host.DependencyTreeViewItems.Add(this);

                LayoutUpdated -= TreeCanvasItemLayoutUpdate;
                LayoutUpdated += TreeCanvasItemLayoutUpdate;
            }
        }

        private void TreeCanvasItemLayoutUpdate(object sender, EventArgs e)
        {
            var viewModel = DataContext as DependencyTreeViewItemViewModel;

            if (viewModel != null)
                foreach (var reference in viewModel.RelatedModel.References)
                {
                    var referenceTreeViewItem = _host.FindReferencedItemOrParent(reference);
                    if (referenceTreeViewItem == null)
                        continue;

                    var from = new Point(0, 0);
                    var controlOne = new Point(0, 0);
                    var controlTwo = new Point(0, 0);
                    var to = referenceTreeViewItem.TranslatePoint(new Point(0, 0), this);

                    var xOffset = 30;
                    var yOffset = 10;

                    from.Y += yOffset;
                    to.Y = from.Y > to.Y ? to.Y + yOffset : to.Y - yOffset;

                    controlOne.X = from.X - xOffset;
                    controlOne.Y = from.Y;

                    controlTwo.X = to.X - xOffset;
                    controlTwo.Y = to.Y;

                    if (_drawedLines.ContainsKey(reference.Id))
                    {
                        var adorner = _drawedLines[reference.Id];
                        adorner.From = from;
                        adorner.ControlTwo = controlOne;
                        adorner.ControlTwo = controlTwo;
                        adorner.To = to;
                        adorner.UpdateLayout();
                    } else {
                        var adorner = new BezierCurveAdorner(this, from, controlOne, controlTwo, to);
                        OutLines.Add(adorner);

                        _host.OutReferencesAdornerLayer.Add(adorner);
                        _drawedLines.Add(reference.Id, adorner);
                    }
                }
        }
    }
}