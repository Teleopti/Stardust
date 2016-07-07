using System;
using System.Globalization;
using System.Windows.Data;

namespace Teleopti.Ccc.WinCode.Converters.Layers
{
    [ValueConversion(typeof(bool), typeof(double))]
    public class IsMoveAllLayerConverter:IValueConverter
    {
        private  double _valueIfFalse = 1d;
        public  double ValueIfFalse
        {
            get { return _valueIfFalse; }
            set { _valueIfFalse = value; }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (System.Convert.ToBoolean(value, CultureInfo.InvariantCulture)) return System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
           return ValueIfFalse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }

    }
}
