using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Teleopti.Ccc.Win.WpfControls.Converters
{
    /// <summary>
    /// Converts a bool to Visibility so you can show items dependning on a bool
    /// </summary>
    /// <remarks>
    /// Returns Collapsed if false
    /// Created by: henrika
    /// Created date: 2009-06-03
    /// </remarks>
    public class VisibilityConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility) return value;
            return ((bool) value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (((Visibility) value == Visibility.Visible));
        }
    }

    public class DebugConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
    

    /// <summary>
    /// Converts a bool to Visibility so you can show items dependning on a bool
    /// </summary>
    /// <remarks>
    /// Returns Collapsed if true
    /// Created by: henrika
    /// Created date: 2009-06-03
    /// </remarks>
    public class ReversedVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility) return value;
            return ((bool)value) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Visibility)value != Visibility.Visible;
        }
    }
}

