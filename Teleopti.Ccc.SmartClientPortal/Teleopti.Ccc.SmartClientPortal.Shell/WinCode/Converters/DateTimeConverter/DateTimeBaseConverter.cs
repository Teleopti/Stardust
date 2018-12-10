using System;
using System.Globalization;
using System.Windows;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Converters.DateTimeConverter
{
    /// <summary>
    /// BaseClass for DateTimeConverting, for handling TimeZones
    /// Can be used as a multibinding by setting the TimeZone as the second value
    /// </summary>
    public abstract class DateTimeBaseConverter : DependencyObject, IDateTimeBaseConverter
    {
        public TimeZoneInfo ConverterTimeZone
        {
            get { return (TimeZoneInfo)GetValue(ConverterTimeZoneProperty); }
            set { SetValue(ConverterTimeZoneProperty, value); }
        }

        public static readonly DependencyProperty ConverterTimeZoneProperty =
            DependencyProperty.Register("ConverterTimeZone",
            typeof(TimeZoneInfo),
            typeof(DateTimeBaseConverter),
            new FrameworkPropertyMetadata(TimeZoneInfo.Local));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CheckDateTime(ref value);
            DateTime dateTimeToConvert = (DateTime)value;
            DateTime retValue = TimeZoneInfo.ConvertTimeFromUtc(dateTimeToConvert, ConverterTimeZone);

            return Transform(retValue,parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CheckDateTime(ref value);
            //If already utc, just return the value
            DateTime toConvertBack = (DateTime)value;
            if (toConvertBack.Kind == DateTimeKind.Utc) return TransformBack(toConvertBack,parameter);

            DateTime retValue = TimeZoneHelper.ConvertToUtc(toConvertBack, ConverterTimeZone);

            return TransformBack(retValue,parameter);
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2)
            {
                CheckTimeZoneInfoOrNull(values[1]);
                if (values[1]!=null) ConverterTimeZone = values[1] as TimeZoneInfo;
                return Convert(values[0], targetType, parameter, culture);
            }
            return Convert(values[0], targetType, parameter, culture);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            DateTime retDate = (DateTime)ConvertBack(value, targetTypes[0], parameter, culture);
            return new object[] { retDate, ConverterTimeZone };
        }

        public abstract object Transform(DateTime convertedDateTime, object parameter);
       
        public abstract object TransformBack(DateTime convertedDateTime, object parameter);

        private static void CheckDateTime(ref object value)
        {
            //Note: Find where this is called instead of doing this, somewhere in the intraday!!!!!!!! (and in Scheduler as well actually, bug no 9266)
            if (value == null || value.Equals(DependencyProperty.UnsetValue))
            {
                value = new DateTime();
            }
            InParameter.MustBeTrue("value", value.GetType() == typeof(DateTime));
        }

        private static void CheckTimeZoneInfoOrNull(object value)
        {
            if (value != null) InParameter.MustBeTrue("value", value.GetType() == typeof(TimeZoneInfo));
        }
    }
}
