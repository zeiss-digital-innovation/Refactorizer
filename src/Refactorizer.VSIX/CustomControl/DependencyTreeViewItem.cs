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

        private Dictionary<Guid, BezierCurveAdorner> _outReferenceArdoner = new Dictionary<Guid, BezierCurveAdorner>();

        private Dictionary<Guid, BezierCurveAdorner> _inReferenceArdoner = new Dictionary<Guid, BezierCurveAdorner>();

        public List<DependencyTreeViewItem> InReferenceArdoner = new List<DependencyTreeViewItem>();

        private readonly Dictionary<Guid, TreeViewItem> _indirectRefererencedTreeViewItems =
            new Dictionary<Guid, TreeViewItem>();

        public DependencyTreeViewItem(DependencyTreeView host)
        {
            _host = host;
            DataContextChanged += TreeCanvasItemDataContextChanged;
        }

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
                            if (childrenOfReference.Count > 0 && _outReferenceArdoner.ContainsKey(reference.Id))
                            {
                                DeleteArdoner(reference.Id);
                            }

                            foreach (var child in childrenOfReference)
                            {
                                var childDataModel = child.DataContext as DependencyTreeViewItemViewModel;
                                if (childDataModel == null) continue;

                                var model = childDataModel.RelatedModel;
                                CreateOutRefrenceArdoner(child, relatedModel, model);

                                if (_indirectRefererencedTreeViewItems.ContainsKey(model.Id)) continue;
                                _indirectRefererencedTreeViewItems.Add(model.Id, child);
                            }

                            if (childrenOfReference.Count == 0)
                            {
                                CreateOutRefrenceArdoner(referenceTreeViewItem, relatedModel, reference);
                            }
                        }
                        else
                        {
                            CreateOutRefrenceArdoner(referenceTreeViewItem, relatedModel, reference);
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
            if (!_outReferenceArdoner.ContainsKey(id))
                return;

            var adorner = _outReferenceArdoner[id];
            _outReferenceArdoner.Remove(id);
            _host.OutReferencesAdornerLayer.Remove(adorner);
        }

        // TODO: Avoid duplicate adordner, add weight to increate line width if mutliple references between the same dependencies
        private BezierCurveAdorner CreateOutRefrenceArdoner(DependencyTreeViewItem referenceTreeViewItem, IModel relatedModel,
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
                fromXOffset = xOffset * (relatedModel is Method ? 4 : relatedModel is Class ? 3 : relatedModel is Namespace ? 2 : 1);
                toXOffset = xOffset * (reference is Method ? 4 : reference is Class ? 3 : reference is Namespace ? 2 : 1);
            }

            controlOne.X = @from.X - fromXOffset;
            controlOne.Y = @from.Y;

            controlTwo.X = to.X - toXOffset;
            controlTwo.Y = to.Y;

            if (_outReferenceArdoner.ContainsKey(reference.Id))
            {
                adorner = _outReferenceArdoner[reference.Id];

                // Adding this item to InReference of referenced tree view item to draw the backline
                referenceTreeViewItem.InReferenceArdoner.Add(this);
               
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
                _host.OutReferencesAdornerLayer.Add(adorner);
                _outReferenceArdoner.Add(reference.Id, adorner);
            }

            return adorner;
        }

        private void DeleteLines()
        {
            foreach (var keyValuePair in _outReferenceArdoner)
                _host.OutReferencesAdornerLayer.Remove(keyValuePair.Value);

            _host.OutReferencesAdornerLayer.UpdateLayout();

            _outReferenceArdoner = new Dictionary<Guid, BezierCurveAdorner>();
        }
    }
}