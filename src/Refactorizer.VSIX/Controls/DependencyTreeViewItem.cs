using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Refactorizer.VSIX.Models;
using Refactorizer.VSIX.View;

namespace Refactorizer.VSIX.Controls
{
    internal class DependencyTreeViewItem : TreeViewItem
    {
        private readonly DependencyTreeView _host;

        private bool _changed;

        private readonly Dictionary<Guid, TreeViewItem> _indirectRefererencedTreeViewItems = new Dictionary<Guid, TreeViewItem>();

        private Dictionary<Guid, BezierCurveAdorner> _inReferenceArdoner = new Dictionary<Guid, BezierCurveAdorner>();

        private Dictionary<Guid, BezierCurveAdorner> _outReferenceArdoner = new Dictionary<Guid, BezierCurveAdorner>();

        public List<DependencyTreeViewItem> InReferenceArdoner = new List<DependencyTreeViewItem>();

        public DependencyTreeViewItem Root { get; set; }

        public List<DependencyTreeViewItem> Childrens { get; set; }

        public DependencyTreeViewItem(DependencyTreeView host)
        {
            _host = host;
            DataContextChanged += TreeCanvasItemDataContextChanged;
            BindEvents(this);
        }

        private void BindEvents(DependencyTreeViewItem view)
        {
            view.Expanded += TreeViewItemExpanded;
            view.Expanded -= TreeViewItemExpanded;
            view.Collapsed -= TreeViewItemCollapse;
            view.Collapsed += TreeViewItemCollapse;
            view.Selected -= TreeViewItemSelected;
            view.Selected += TreeViewItemSelected;
            view.Unselected -= TreeViewItemSelected;
            view.Unselected += TreeViewItemSelected;
        }

        private void TreeViewItemSelected(object sender, RoutedEventArgs e)
        {
            _changed = true;
        }

        private void TreeViewItemExpanded(object sender, RoutedEventArgs e)
        {
            _changed = true;
        }

        private void TreeViewItemCollapse(object sender, RoutedEventArgs e)
        {
            _changed = true;
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
            if (!_host.Contains(this))
            {
                _host.AddTreeViewItem(this);

                _changed = true;
                LayoutUpdated -= TreeCanvasItemLayoutUpdate;
                LayoutUpdated += TreeCanvasItemLayoutUpdate;
            }
        }

        private void TreeCanvasItemLayoutUpdate(object sender, EventArgs e)
        {
            var viewModel = DataContext as DependencyTreeItemView;
            if (viewModel == null)
                return;

            var relatedModel = viewModel.RelatedModel;

            // As there is no hide event, we need to do this check all time
            if ((IsExpanded || !relatedModel.HasChildren || !IsVisible) && _outReferenceArdoner.Count > 0)
                DeleteLines();

            if (!_changed)
                return;

            _changed = false;

            var references = relatedModel.References;

            var @class = viewModel.RelatedModel as Class;
            if (@class != null)
            {
                references = @class.Fields.Aggregate(references,
                    (current, field) => current.Union(field.References).ToList());
                references = @class.Properties.Aggregate(references,
                    (current, property) => current.Union(property.References).ToList());
                references = @class.Methods.Aggregate(references, 
                    (current, method) => current.Union(method.References).ToList());
            }

            foreach (var reference in references)
            {
                var referenceTreeViewItem = _host.FindReferencedItemOrParent(reference);
                if (referenceTreeViewItem == null)
                    continue;

                //if (referenceTreeViewItem.IsExpanded)
                //{
                //    if (viewModel.HasDummyChild)
                //        viewModel.RemoveDummyAndLoadChildren();

                //    var childrenOfReference =
                //        _host.FindLastExpandedDependencyTreeViewItems(referenceTreeViewItem);

                //    if (childrenOfReference.Count > 0 && _outReferenceArdoner.ContainsKey(reference.Id))
                //        DeleteArdoner(reference.Id);

                //    foreach (var child in childrenOfReference)
                //    {
                //        var childDataModel = child.DataContext as DependencyTreeViewItemViewModel;
                //        if (childDataModel == null) continue;
                        
                //        // We need to update this item if references updated
                //        BindEvents(child);

                //        var model = childDataModel.RelatedModel;
                //        CreateOutRefrenceArdoner(child, relatedModel, model);

                //        if (_indirectRefererencedTreeViewItems.ContainsKey(model.Id)) continue;
                //        _indirectRefererencedTreeViewItems.Add(model.Id, child);
                //    }

                //    if (childrenOfReference.Count == 0)
                //        CreateOutRefrenceArdoner(referenceTreeViewItem, relatedModel, reference);
                //}
                //else
                //{
                    // We need to update this item if references updated
                    BindEvents(referenceTreeViewItem);
                    
                    CreateOutRefrenceArdoner(referenceTreeViewItem, relatedModel, reference);
                //}
            }
            //foreach (var key in _indirectRefererencedTreeViewItems.Keys.ToList())
            //{
            //    var item = _indirectRefererencedTreeViewItems[key];
            //    if (!item.IsVisible || item.IsExpanded)
            //    {
            //        DeleteArdoner(key);
            //        _indirectRefererencedTreeViewItems.Remove(key);
            //    }
            //}
        }

        // TODO: Avoid duplicate adordner, add weight to increate line width if mutliple references between the same dependencies
        private void CreateOutRefrenceArdoner(DependencyTreeViewItem referenceTreeViewItem, IModel relatedModel,
            IModel reference)
        {
            BezierCurveAdorner adorner;
            var xOffset = 30;
            var yOffset = 12;
            var from = new Point(0, 0);
            var controlOne = new Point(0, 0);
            var controlTwo = new Point(0, 0);
            var to = referenceTreeViewItem.TranslatePoint(new Point(0, 0), this);

            from.Y += yOffset;
            to.Y = from.Y > to.Y ? to.Y + yOffset : to.Y - yOffset;

            var fromXOffset = xOffset;
            var toXOffset = xOffset;

            if (relatedModel.Parent != null && !relatedModel.Parent.Id.Equals(reference.Parent.Id))
            {
                fromXOffset = xOffset * (GetOffsetFactor(relatedModel));
                toXOffset = xOffset * (GetOffsetFactor(reference));
            }

            controlOne.X = from.X - fromXOffset;
            controlOne.Y = from.Y;

            controlTwo.X = to.X - toXOffset;
            controlTwo.Y = to.Y;

            if (_outReferenceArdoner.ContainsKey(reference.Id))
            {
                adorner = _outReferenceArdoner[reference.Id];
                //if (adorner.IsSelected != IsSelected)
                //{
                //    DeleteArdoner(reference.Id);
                //    return;
                //}

                // Adding this item to InReference of referenced tree view item to draw the backline
                if (!referenceTreeViewItem.InReferenceArdoner.Contains(this))
                    referenceTreeViewItem.InReferenceArdoner.Add(this);

                // Only update if some some point has changed
                if (adorner.From != from ||
                    adorner.ControlOne != controlOne ||
                    adorner.ControlTwo != controlTwo ||
                    adorner.To != to)
                {
                    adorner.From = from;
                    adorner.ControlOne = controlOne;
                    adorner.ControlTwo = controlTwo;
                    adorner.To = to;
                    adorner.UpdateLayout();
                }
            }
            else
            {
                adorner = new BezierCurveAdorner(this, from, controlOne, controlTwo, to) {IsHitTestVisible = false};
                //adorner.IsSelected = IsSelected;
                _host.OutReferencesAdornerLayer.Add(adorner);
                _outReferenceArdoner.Add(reference.Id, adorner);
            }
        }

        private static int GetOffsetFactor(IModel relatedModel)
        {
            return relatedModel is Method || relatedModel is Field || relatedModel is Property ? 4 
                : relatedModel is Class ? 3 
                : relatedModel is Namespace ? 2 
                : 1;
        }

        private void DeleteLines()
        {
            foreach (var keyValuePair in _outReferenceArdoner)
                _host.OutReferencesAdornerLayer.Remove(keyValuePair.Value);

            _host.OutReferencesAdornerLayer.UpdateLayout();

            _outReferenceArdoner = new Dictionary<Guid, BezierCurveAdorner>();
        }

        private void DeleteArdoner(Guid id)
        {
            if (!_outReferenceArdoner.ContainsKey(id))
                return;

            var adorner = _outReferenceArdoner[id];
            _outReferenceArdoner.Remove(id);
            _host.OutReferencesAdornerLayer.Remove(adorner);
        }
    }
}