using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Refactorizer.VSIX.Models;
using Refactorizer.VSIX.View;

namespace Refactorizer.VSIX.Controls
{
    internal class DependencyTreeItemControl : TreeViewItem
    {
        private readonly DependencyTreeControl _host;

        private Dictionary<Guid, BezierCurveAdorner> _inReferenceArdoner = new Dictionary<Guid, BezierCurveAdorner>();

        private Dictionary<Guid, BezierCurveAdorner> _outReferenceArdoner = new Dictionary<Guid, BezierCurveAdorner>();

        public List<DependencyTreeItemControl> ReferenceControls = new List<DependencyTreeItemControl>();

        private ElipseAdorner _itemAlias;
        private int _yOffset = 12;

        public DependencyTreeItemControl(DependencyTreeControl host)
        {
            _host = host;
            DataContextChanged += TreeCanvasItemDataContextChanged;
        }

        public DependencyTreeItemControl Root { get; set; }

        public List<DependencyTreeItemControl> Childrens { get; set; } = new List<DependencyTreeItemControl>();

        /// <summary>
        ///     Set the child item type
        /// </summary>
        /// <returns></returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new DependencyTreeItemControl(_host);
        }

        private void TreeCanvasItemDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!_host.Contains(this))
                _host.AddTreeViewItem(this);

            LayoutUpdated -= TreeCanvasItemLayoutUpdate;
            LayoutUpdated += TreeCanvasItemLayoutUpdate;
        }

        private void TreeCanvasItemLayoutUpdate(object sender, EventArgs e)
        {
            var viewModel = DataContext as DependencyTreeItemView;
            if (viewModel == null)
                return;

            if (IsVisible && _itemAlias == null)
                DrawItemAlias();
            else if (!IsVisible && _itemAlias != null)
                DeleteItemAlias();

            // As there is no hide event, we need to do this check all time
            if (!IsVisible || (IsExpanded && Childrens.Any()))
            {
                DeleteAllAdorners();
                return;
            }

            var relatedModel = viewModel.RelatedModel;

            foreach (var reference in GetOutReferences(relatedModel))
            {
                var referenceTreeItemControl = _host.FindReferencedItemOrParent(reference);
                if (referenceTreeItemControl == null || referenceTreeItemControl.Equals(this))
                    continue;

                CreateOrUpdateOutRefrenceArdoner(relatedModel, referenceTreeItemControl, reference);
            }
            foreach (var reference in GetInReferences(relatedModel))
            {
                var referenceTreeItemControl = _host.FindReferencedItemOrParent(reference);
                if (referenceTreeItemControl == null || referenceTreeItemControl.Equals(this))
                    continue;

                CreateOrUpdateInRefrenceArdoner(relatedModel, referenceTreeItemControl, reference);
            }
        }

        private ICollection<IModel> GetInReferences(IModel relatedModel)
        {
            var references = relatedModel.InReferences;

            var @class = relatedModel as Class;
            if (@class != null)
            {
                foreach (var field in @class.Fields)
                foreach (var reference in field.InReferences)
                    if (!references.Contains(reference))
                        references.Add(reference);
                foreach (var property in @class.Properties)
                foreach (var reference in property.InReferences)
                    if (!references.Contains(reference))
                        references.Add(reference);
                foreach (var method in @class.Methods)
                foreach (var reference in method.InReferences)
                    if (!references.Contains(reference))
                        references.Add(reference);
            }
            return references;
        }

        private static ICollection<IModel> GetOutReferences(IModel relatedModel)
        {
            var references = relatedModel.OutReferences;

            var @class = relatedModel as Class;
            if (@class != null)
            {
                foreach (var field in @class.Fields)
                foreach (var reference in field.OutReferences)
                    if (!references.Contains(reference))
                        references.Add(reference);
                foreach (var property in @class.Properties)
                foreach (var reference in property.OutReferences)
                    if (!references.Contains(reference))
                        references.Add(reference);
                foreach (var method in @class.Methods)
                foreach (var reference in method.OutReferences)
                    if (!references.Contains(reference))
                        references.Add(reference);
            }
            return references;
        }

        private void CreateOrUpdateOutRefrenceArdoner(IModel thisModel,
            DependencyTreeItemControl teeItemControlOfReference,
            IModel modelOfReference)
        {
            CreateOrUpdateArdoner(thisModel, teeItemControlOfReference, modelOfReference, _outReferenceArdoner, true);
        }

        private void CreateOrUpdateInRefrenceArdoner(IModel thisModel,
            DependencyTreeItemControl treeItemControlOfReference,
            IModel modelOfReference)
        {
            DeleteInReferences();

            CreateOrUpdateArdoner(thisModel, treeItemControlOfReference, modelOfReference, _inReferenceArdoner, false);
        }

        private void DrawItemAlias()
        {
            var itemAliasPoint = new Point(560, _yOffset);
            _itemAlias = new ElipseAdorner(this, itemAliasPoint);
            _host.AdornerLayer.Add(_itemAlias);
            _host.AdornerLayer.UpdateLayout();
        }

        private void CreateOrUpdateArdoner( IModel thisModel, DependencyTreeItemControl treeItemControlOfReference, IModel modelOfReference, Dictionary<Guid, BezierCurveAdorner> store, bool isLeft)
        {
            BezierCurveAdorner bezierCurveAdorner;
            var toRight = 550 + (isLeft ? 0 : 20);
            var halfRight = Math.Round((double)toRight / 2);
            var positionFactor = isLeft ? -1 : 1;

            var from = new Point(toRight, _yOffset);
            var to = treeItemControlOfReference.TranslatePoint(from, this);
            var controlOne = from;
            var controlTwo = to;

            var offsetFactor = Math.Ceiling((to.Y - from.Y) / 50);
            offsetFactor *= offsetFactor < 0 ? -1 : 1;
            var xOffset = 20 * (offsetFactor > 1 ? offsetFactor > halfRight ? halfRight : offsetFactor : 1) * positionFactor;

            controlOne.X = from.X + xOffset;
            controlOne.Y = from.Y;

            controlTwo.X = to.X + xOffset;
            controlTwo.Y = to.Y;

            if (store.ContainsKey(modelOfReference.Id))
            {
                bezierCurveAdorner = store[modelOfReference.Id];
                if (bezierCurveAdorner.IsSelected != IsSelected)
                {
                    DeleteArdoner(modelOfReference.Id);
                    return;
                }

                // Adding this item to InReference of referenced tree view item to draw the backline
                // Only update if some some point has changed
                if (bezierCurveAdorner.From != from ||
                    bezierCurveAdorner.ControlOne != controlOne ||
                    bezierCurveAdorner.ControlTwo != controlTwo ||
                    bezierCurveAdorner.To != to)
                {
                    bezierCurveAdorner.From = from;
                    bezierCurveAdorner.ControlOne = controlOne;
                    bezierCurveAdorner.ControlTwo = controlTwo;
                    bezierCurveAdorner.To = to;
                    bezierCurveAdorner.UpdateLayout();
                    _host.AdornerLayer.UpdateLayout();
                }
            }
            else
            {
                if (!treeItemControlOfReference.ReferenceControls.Contains(this))
                    treeItemControlOfReference.ReferenceControls.Add(this);

                bezierCurveAdorner = new BezierCurveAdorner(this, from, controlOne, controlTwo, to) { IsHitTestVisible = false };
                bezierCurveAdorner.IsHarmfull = thisModel.IsHarmfull;
                bezierCurveAdorner.IsSelected = IsSelected;

                _host.AdornerLayer.Add(bezierCurveAdorner);
                store.Add(modelOfReference.Id, bezierCurveAdorner);
                _host.AdornerLayer.UpdateLayout();
            }
        }

        private int GetOffsetFactor(IModel firstModel, IModel secondModel)
        {
            if (firstModel.Parent == null && secondModel.Parent == null)
                return 1;

            if (firstModel.Parent != null && secondModel.Parent != null &&
                firstModel.Parent.Id.Equals(secondModel.Parent.Id))
                return 1;

            var firstOffset = GetOffsetFactorHelper(firstModel);
            var secondOffset = GetOffsetFactorHelper(secondModel);
            var sub = firstOffset > secondOffset ? firstOffset - secondOffset : secondOffset - firstOffset;

            return firstOffset - sub;
        }

        private static int GetOffsetFactorHelper(IModel relatedModel)
        {
            return relatedModel is Method || relatedModel is Field || relatedModel is Property
                ? 4
                : relatedModel is Class
                    ? 3
                    : relatedModel is Namespace
                        ? 2
                        : 1;
        }

        private void DeleteItemAlias()
        {
            _host.AdornerLayer.Remove(_itemAlias);
            _itemAlias = null;
            _host.AdornerLayer.UpdateLayout();
        }

        private void DeleteAllAdorners()
        {
            foreach (var keyValuePair in _outReferenceArdoner)
                _host.AdornerLayer.Remove(keyValuePair.Value);
            _outReferenceArdoner = new Dictionary<Guid, BezierCurveAdorner>();

            DeleteInReferences();

            _host.AdornerLayer.UpdateLayout();
        }

        private void DeleteInReferences()
        {
            foreach (var keyValuePair in _inReferenceArdoner)
                _host.AdornerLayer.Remove(keyValuePair.Value);
            _inReferenceArdoner = new Dictionary<Guid, BezierCurveAdorner>();
        }

        private void DeleteArdoner(Guid id)
        {
            if (_outReferenceArdoner.ContainsKey(id))
            {
                var adorner = _outReferenceArdoner[id];
                _outReferenceArdoner.Remove(id);
                _host.AdornerLayer.Remove(adorner);
            }

            if (_inReferenceArdoner.ContainsKey(id))
            {
                var adorner = _inReferenceArdoner[id];
                _inReferenceArdoner.Remove(id);
                _host.AdornerLayer.Remove(adorner);
            }
        }
    }
}