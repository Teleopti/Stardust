using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Teleopti.Ccc.WpfControls.Converters
{
    /// <summary>
    /// Converter to return a gradient brush from System.Drawing.Color
    /// </summary>
    [ValueConversion(typeof(System.Drawing.Color), typeof(LinearGradientBrush), ParameterType = typeof(string))]
    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Drawing.Color)
            {
                System.Drawing.Color colorToConvert = (System.Drawing.Color)value;

                Color baseColor = Color.FromRgb(colorToConvert.R, colorToConvert.G, colorToConvert.B);
                return new LinearGradientBrush(Colors.White, baseColor, new Point(0, 0), new Point(0, 0.1));


            }
        	//TODO: Change to use internaltexts:
        	throw new ArgumentException("The argument for this ColorConverter is not a System.Drawing.Color", "value");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //Dont use for now.
            return null;
        }
    }
}
