﻿using System;
using System.Windows.Data;

namespace Teleopti.Ccc.WpfControls.Converters
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    class TextConverter :IValueConverter
    {

        #region IValueConverter Members
      
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return UserTexts.Resources.ResourceManager.GetString((string)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
