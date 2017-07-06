using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Refactorizer.VSIX.Models;
using Refactorizer.VSIX.ViewModels;

namespace Refactorizer.VSIX.CustomControl
{
    internal class DependencyTreeViewItem : TreeViewItem
    {
        private readonly DependencyTreeView _host;

        private Dictionary<Guid, BezierCurveAdorner> _drawedLines = new Dictionary<Guid, BezierCurveAdorner>();

        private readonly Dictionary<Guid, TreeViewItem> _indirectRefererencedTreeViewItems =
            new Dictionary<Guid, TreeViewItem>();

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
                if (IsExpanded || !IsVisible)
                {
                    DeleteLines();
                }
                else
                {
                    var relatedModel = viewModel.RelatedModel;

                    foreach (var reference in relatedModel.References)
                    {
                        var referenceTreeViewItem = _host.FindReferencedItemOrParent(reference);
                        if (referenceTreeViewItem == null)
                            continue;

                        if (referenceTreeViewItem.IsExpanded)
                        {
                            if (viewModel.HasDummyChild)
                                viewModel.RemoveDummyAndLoadChildren();

                            var childrenOfReference =
                                _host.FindLastExpandedDependencyTreeViewItems(referenceTreeViewItem);
                            if (childrenOfReference.Count > 0 && _drawedLines.ContainsKey(reference.Id))
                            {
                                DeleteArdoner(reference.Id);
                            }

                            foreach (var child in childrenOfReference)
                            {
                                var childDataModel = child.DataContext as DependencyTreeViewItemViewModel;
                                if (childDataModel == null) continue;

                                var model = childDataModel.RelatedModel;
                                CreateAdroner(child, relatedModel, model);

                                if (_indirectRefererencedTreeViewItems.ContainsKey(model.Id)) continue;
                                _indirectRefererencedTreeViewItems.Add(model.Id, child);
                            }
                        }
                        else
                        {
                            CreateAdroner(referenceTreeViewItem, relatedModel, reference);
                        }
                    }
                    foreach (var key in _indirectRefererencedTreeViewItems.Keys.ToList())
                    {
                        var item = _indirectRefererencedTreeViewItems[key];
                        if (!item.IsVisible || item.IsExpanded)
                        {
                            DeleteArdoner(key);
                            _indirectRefererencedTreeViewItems.Remove(key);
                        }
                    }
                }
        }

        private void DeleteArdoner(Guid id)
        {
            if (!_drawedLines.ContainsKey(id))
                return;

            var adorner = _drawedLines[id];
            _drawedLines.Remove(id);
            _host.OutReferencesAdornerLayer.Remove(adorner);
        }

        private BezierCurveAdorner CreateAdroner(DependencyTreeViewItem referenceTreeViewItem, IModel relatedModel,
            IModel reference)
        {
            BezierCurveAdorner adorner;
            var xOffset = 30;
            var yOffset = 12;
            var from = new Point(0, 0);
            var controlOne = new Point(0, 0);
            var controlTwo = new Point(0, 0);
            var to = referenceTreeViewItem.TranslatePoint(new Point(0, 0), this);

            @from.Y += yOffset;
            to.Y = @from.Y > to.Y ? to.Y + yOffset : to.Y - yOffset;

            var fromXOffset = xOffset;
            var toXOffset = xOffset;

            if (relatedModel.Parent != null && !relatedModel.Parent.Id.Equals(reference.Parent.Id))
            {
                fromXOffset = xOffset * (relatedModel is Class ? 3 : relatedModel is Namespace ? 2 : 1);
                toXOffset = xOffset * (reference is Class ? 3 : reference is Namespace ? 2 : 1);
            }

            controlOne.X = @from.X - fromXOffset;
            controlOne.Y = @from.Y;

            controlTwo.X = to.X - toXOffset;
            controlTwo.Y = to.Y;

            if (_drawedLines.ContainsKey(reference.Id))
            {
                adorner = _drawedLines[reference.Id];
                // Only update if some some point has changed
                if (adorner.From != @from ||
                    adorner.ControlOne != controlOne ||
                    adorner.ControlTwo != controlTwo ||
                    adorner.To != to)
                {
                    adorner.From = @from;
                    adorner.ControlOne = controlOne;
                    adorner.ControlTwo = controlTwo;
                    adorner.To = to;
                    adorner.UpdateLayout();
                }
            }
            else
            {
                adorner = new BezierCurveAdorner(this, @from, controlOne, controlTwo, to) {IsHitTestVisible = false};
                // Make adorner not clickable, otherwise we could get in trouble open or closing a tree item
                OutLines.Add(adorner);

                _host.OutReferencesAdornerLayer.Add(adorner);
                _drawedLines.Add(reference.Id, adorner);
            }

            return adorner;
        }

        private void DeleteLines()
        {
            foreach (var keyValuePair in _drawedLines)
                _host.OutReferencesAdornerLayer.Remove(keyValuePair.Value);

            _host.OutReferencesAdornerLayer.UpdateLayout();

            _drawedLines = new Dictionary<Guid, BezierCurveAdorner>();
        }
    }
}