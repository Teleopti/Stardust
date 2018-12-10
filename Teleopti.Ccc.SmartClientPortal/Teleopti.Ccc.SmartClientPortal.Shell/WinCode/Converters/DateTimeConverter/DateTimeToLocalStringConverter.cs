using System;
using System.Globalization;
using System.Windows.Data;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Converters.DateTimeConverter
{
    /// <summary>
    /// Converter for converting a string to utc datetime (and back)
    /// </summary>
    /// <remarks>
    /// This is just a combination of a datetimetolocalconverter and a parser
    /// It sets its value to the latest parsable datetime
    /// </remarks>
    public class DateTimeToLocalStringConverter:IMultiValueConverter
    {
        public static bool IsDesignTime { get; set; }

        public static DateTime DefaultDateTime
        {
            get { return DateTime.UtcNow; }
        }
    

        /// <summary>
        /// Gets or sets the latest converted date time.
        /// </summary>
        /// <value>The latest converted date time.</value>
        /// <remarks>
        /// Used for resetting the value if not able to parse the string
        /// </remarks>
        public DateTime LatestConvertedDateTime { get; private set; }

        public IDateTimeParser Parser { get; set; }
        private readonly DateTimeToLocalConverter _converter = new DateTimeToLocalConverter();

        public DateTimeToLocalStringConverter()
        {
            Parser = new DateTimeParser();
            LatestConvertedDateTime = DateTime.Now;
        }

        /// <summary>
        /// Converts the specified value up to the gui
        /// </summary>
        /// <param name="values">The array of values that the source bindings in the <see cref="T:System.Windows.Data.MultiBinding"/> produces. The value <see cref="F:System.Windows.DependencyProperty.UnsetValue"/> indicates that the source binding has no value to provide for conversion.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>
        /// A converted value.
        /// If the method returns null, the valid null value is used.
        /// A return value of <see cref="T:System.Windows.DependencyProperty"/>.<see cref="F:System.Windows.DependencyProperty.UnsetValue"/> indicates that the converter did not produce a value, and that the binding will use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue"/> if it is available, or else will use the default value.
        /// A return value of <see cref="T:System.Windows.Data.Binding"/>.<see cref="F:System.Windows.Data.Binding.DoNothing"/> indicates that the binding does not transfer the value or use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue"/> or the default value.
        /// </returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (IsDesignTime) return values[0];
            InParameter.MustBeTrue("values", values.Length==2);
            InParameter.MustBeTrue("values", values[0] is DateTime);
            _converter.ConverterTimeZone = (TimeZoneInfo) values[1];
            DateTime local =   (DateTime)_converter.Convert(values[0], targetType, parameter, culture);
            LatestConvertedDateTime = local;
            return Parser.ToGuiText(local, Mode(parameter)); 
        }

        /// <summary>
        /// Converts the back the value to the object
        /// </summary>
        /// <param name="value">The value that the binding target produces.</param>
        /// <param name="targetTypes">The array of types to convert to. The array length indicates the number and types of values that are suggested for the method to return.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>
        /// An array of values that have been converted from the target value back to the source values.
        /// </returns>
        /// <remarks>
        /// Converts it to Utc using DateTimeToLocalConverter
        /// Parses the string
        /// </remarks>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            if(value != null)
                LatestConvertedDateTime = Parser.Parse(value.ToString(), LatestConvertedDateTime, Mode(parameter));
            var utcDateTime =
                (DateTime) _converter.ConvertBack(LatestConvertedDateTime, typeof (DateTime), parameter, culture);

            //Now we have a valid datetime in local format, use the converter to set it to utc:
            return new object[] {utcDateTime, _converter.ConverterTimeZone};

            
        }
        private static DateTimeParseMode Mode(object parameter)
            {
                return (parameter is DateTimeParseMode) ? (DateTimeParseMode) parameter : DateTimeParseMode.DateTime;
            }

    }
}
