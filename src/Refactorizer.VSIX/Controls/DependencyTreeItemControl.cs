using System;
using System.Diagnostics;
using System.Collections.Generic;
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

        public bool Changed;

        private Dictionary<Guid, BezierCurveAdorner> _inReferenceArdoner = new Dictionary<Guid, BezierCurveAdorner>();

        private Dictionary<Guid, BezierCurveAdorner> _outReferenceArdoner = new Dictionary<Guid, BezierCurveAdorner>();

        public List<DependencyTreeItemControl> ReferenceControls = new List<DependencyTreeItemControl>();

        public DependencyTreeItemControl Root { get; set; }

        public List<DependencyTreeItemControl> Childrens { get; set; } = new List<DependencyTreeItemControl>();

        public DependencyTreeItemControl(DependencyTreeControl host)
        {
            Changed = true;
            _host = host;
            DataContextChanged += TreeCanvasItemDataContextChanged;
            Expanded += TreeViewItemUpdate;
            Collapsed += TreeViewItemUpdate;
            Selected += TreeViewItemUpdate;
            Unselected += TreeViewItemUpdate;
        }

        private void TreeViewItemUpdate(object sender, RoutedEventArgs e)
        {
            if (Changed == false)
            {
                Changed = true;

                foreach (var children in Childrens)
                    children.Changed = true;

                foreach (var control in ReferenceControls)
                    control.Changed = true;
            }
        }

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

            Changed = true;
            LayoutUpdated -= TreeCanvasItemLayoutUpdate;
            LayoutUpdated += TreeCanvasItemLayoutUpdate;
        }

        private void TreeCanvasItemLayoutUpdate(object sender, EventArgs e)
        {
            var viewModel = DataContext as DependencyTreeItemView;
            if (viewModel == null)
                return;

            // As there is no hide event, we need to do this check all time
            if ((IsExpanded || !IsVisible) && Childrens.Any() && _outReferenceArdoner.Count > 0)
            {
                DeleteLines();
                return;
            }

            if (!Changed)
                return;

            Changed = false;

            var relatedModel = viewModel.RelatedModel;
            var references = GetReferences(relatedModel);

            Trace.WriteLine($"Update {relatedModel.Name}");

            foreach (var reference in references)
            {
                var referenceTreeItemControl = _host.FindReferencedItemOrParent(reference);
                if (referenceTreeItemControl == null || referenceTreeItemControl.Equals(this))
                {
                    continue;
                }
                Trace.WriteLine($"{reference.Name}");

                CreateOutRefrenceArdoner(relatedModel, referenceTreeItemControl, reference);
            }

            Trace.WriteLine($"----------------------------------------------------");
        }

        private static ICollection<IModel> GetReferences(IModel relatedModel)
        {
            var references = relatedModel.References;

            var @class = relatedModel as Class;
            if (@class != null)
            {
                foreach (var field in @class.Fields)
                {
                    foreach (var reference in field.References)
                    {
                        if (!references.Contains(reference))
                            references.Add(reference);
                    }
                }
                foreach (var property in @class.Properties)
                {
                    foreach (var reference in property.References)
                    {
                        if (!references.Contains(reference))
                            references.Add(reference);
                    }
                }
                foreach (var method in @class.Methods)
                {
                    foreach (var reference in method.References)
                    {
                        if (!references.Contains(reference))
                            references.Add(reference);
                    }
                }
            }
            return references;
        }

        private void CreateOutRefrenceArdoner(IModel relatedModel, DependencyTreeItemControl referenceTreeItemControl, IModel referenceModel)
        {
            BezierCurveAdorner adorner;
            var xOffset = 30;
            var yOffset = 12;
            var from = new Point(0, 0);
            var controlOne = new Point(0, 0);
            var controlTwo = new Point(0, 0);
            var to = referenceTreeItemControl.TranslatePoint(new Point(0, 0), this);

            from.Y += yOffset;
            to.Y = from.Y > to.Y ? to.Y + yOffset : to.Y - yOffset;

            var fromXOffset = xOffset;
            var toXOffset = xOffset;

            if (relatedModel.Parent != null && !relatedModel.Parent.Id.Equals(referenceModel.Parent.Id))
            {
                fromXOffset = xOffset * (GetOffsetFactor(relatedModel));
                toXOffset = xOffset * (GetOffsetFactor(referenceModel));
            }

            controlOne.X = from.X - fromXOffset;
            controlOne.Y = from.Y;

            controlTwo.X = to.X - toXOffset;
            controlTwo.Y = to.Y;

            if (_outReferenceArdoner.ContainsKey(referenceModel.Id))
            {
                adorner = _outReferenceArdoner[referenceModel.Id];
                if (adorner.IsSelected != IsSelected)
                {
                    DeleteArdoner(referenceModel.Id);
                    Changed = true;
                    return;
                }

                // Adding this item to InReference of referenced tree view item to draw the backline
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
                if (!referenceTreeItemControl.ReferenceControls.Contains(this))
                    referenceTreeItemControl.ReferenceControls.Add(this);

                adorner = new BezierCurveAdorner(this, from, controlOne, controlTwo, to) { IsHitTestVisible = false };
                adorner.IsSelected = IsSelected;
                _host.OutReferencesAdornerLayer.Add(adorner);
                _outReferenceArdoner.Add(referenceModel.Id, adorner);
                _host.OutReferencesAdornerLayer.UpdateLayout();
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