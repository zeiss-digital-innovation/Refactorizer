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

            // For debug 
            var p1 = new Pen(new SolidColorBrush(Colors.DarkViolet), 1.5);
            var p2 = new Pen(new SolidColorBrush(Colors.DodgerBlue), 1.5);
            var p3 = new Pen(new SolidColorBrush(Colors.White), 1.5);
            var p4 = new Pen(new SolidColorBrush(Colors.GreenYellow), 1.5);
            drawingContext.DrawEllipse(Brushes.Transparent, p1, From, 1, 1);
            drawingContext.DrawEllipse(Brushes.Transparent, p2, ControlOne, 1, 1);
            drawingContext.DrawEllipse(Brushes.Transparent, p3, ControlTwo, 1, 1);
            drawingContext.DrawEllipse(Brushes.Transparent, p4, To, 1, 1);
        }
    }
}