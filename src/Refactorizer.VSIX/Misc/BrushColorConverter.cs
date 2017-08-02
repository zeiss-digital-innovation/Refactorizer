using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Refactorizer.VSIX.Controls;

namespace Refactorizer.VSIX.Misc
{
    class BrushColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = value as BezierCurveAdorner;
            if (item != null && item.IsSelected)
                return new SolidColorBrush(Colors.OrangeRed);

            return new SolidColorBrush(Colors.White);

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
