using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Data;

namespace Teleopti.Ccc.WinCode.Converters
{
    /// <summary>
    /// SimpleConverter returns the same value as is sent in, can be used for tracking databindings
    /// and MUST be used when binding TwoWay using NotifyPropertyChanged and transforming on the setter.
    /// Like Validation that changes the actual value, otherwise the GUI will not update
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2008-12-10
    /// </remarks>
    [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
    public class SimpleConverter :IValueConverter
    {
       
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
           
            return value;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
          
            return value;
        }
    }
}
