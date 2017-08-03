using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Refactorizer.VSIX.Adorners
{
    class ElipseAdorner : Adorner
    {
        public ElipseAdorner(UIElement adornedElement, Point center) : base(adornedElement)
        {
            Center = center;
        }

        public bool IsSelected { get; set; }

        public Point Center { get; set; }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var color = IsSelected ? Colors.OrangeRed : Colors.White;
            drawingContext.DrawEllipse(new SolidColorBrush(color), new Pen(new SolidColorBrush(color), 1), Center, 5.0, 5.0);
        }
    }
}