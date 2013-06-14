using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Converters
{
    /// <summary>
    /// Return null if layer is null or layer.Period.LocalStartDateTime
    /// </summary>
    public class LayerPeriodConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            var layer = (ILayer<IPayload>)value;
            return layer.Period.LocalStartDateTime;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
