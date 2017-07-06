using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Refactorizer.VSIX.CustomControl
{
    class BezierCurveAdorner : Adorner

    {
        public BezierCurveAdorner(UIElement adornedElement, Point from, Point controlOne, Point controlTwo, Point to) : base(adornedElement)
        {
            From = from;
            ControlOne = controlOne;
            ControlTwo = controlTwo;
            To = to;
        }

        public Point From { get; set; }

        public Point ControlOne { get; set; }

        public Point ControlTwo { get; set; }

        public Point To { get; set; }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var pen = new Pen(new SolidColorBrush(Colors.OrangeRed), 1.5);

            var pathFigure = new PathFigure();
            pathFigure.StartPoint = From;
            pathFigure.Segments.Add(new BezierSegment(ControlOne, ControlTwo, To, true));

            var pathGeometry = new PathGeometry(new[] {pathFigure});

            drawingContext.DrawGeometry(Brushes.Transparent, pen, pathGeometry);
        }
    }
}