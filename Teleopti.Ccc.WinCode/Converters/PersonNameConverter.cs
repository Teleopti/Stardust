using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Converters
{
    public class PersonNameConverter : IValueConverter
    {
        public static CommonNameDescriptionSetting Setting { get; set; }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IPerson person = (IPerson)value;
            return Setting.BuildCommonNameDescription(person);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        #endregion
    }
}