using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Refactorizer.VSIX.Controls
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

        public bool IsSelected { get; set; }

        public bool IsHarmfull { get; set; }

        public Point From { get; set; }

        public Point ControlOne { get; set; }

        public Point ControlTwo { get; set; }

        public Point To { get; set; }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var color = IsSelected ? Colors.DodgerBlue : IsHarmfull ? Colors.OrangeRed : Colors.White;
            var brush = new SolidColorBrush(color) {Opacity = IsSelected ? 1 : 0.5};
            var pen = new Pen(brush, 1.5);

            var pathFigure = new PathFigure();
            pathFigure.StartPoint = From;
            pathFigure.Segments.Add(new BezierSegment(ControlOne, ControlTwo, To, true));

            var pathGeometry = new PathGeometry(new[] { pathFigure });
            drawingContext.DrawGeometry(Brushes.Transparent, pen, pathGeometry);
            drawingContext.DrawEllipse(new SolidColorBrush(color), new Pen(new SolidColorBrush(color), 1), From, 1.5, 1.5);

            //drawingContext.DrawEllipse(new SolidColorBrush(Colors.Red), new Pen(new SolidColorBrush(Colors.Red), 1), ControlOne, 1.0, 1.0);
            //drawingContext.DrawEllipse(new SolidColorBrush(Colors.Green), new Pen(new SolidColorBrush(Colors.Green), 1), ControlTwo, 1.0, 1.0);
            //drawingContext.DrawEllipse(new SolidColorBrush(Colors.Blue), new Pen(new SolidColorBrush(Colors.Blue), 1), To, 1.0, 1.0);
        }
    }
}