using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Teleopti.Ccc.Win.WpfControls.Converters
{
    /// <summary>
    /// Converter to retrun a color from a string
    /// </summary>
    [ValueConversion(typeof(string), typeof(SolidColorBrush), ParameterType = typeof(string))]
    public class StringToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            if (System.Drawing.Color.FromName(value.ToString()).IsKnownColor)
            {
                var drawingColor = System.Drawing.Color.FromName(value.ToString());
                return new SolidColorBrush(Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G,
                                                                     drawingColor.B));
            }
            return new SolidColorBrush
            {
                Color = new Color(),
                Opacity = 0
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new SolidColorBrush
            {
                Color = new Color(),
                Opacity = 0
            };
        }
    }
}
