using System;
using System.Windows.Data;

namespace Teleopti.Ccc.WinCode.Converters.DateTimeConverter
{
    /// <summary>
    /// For DateTimeConverting, for handling visual TimeZones
    /// Can be used as a multibinding by setting the TimeZone as the second value
    /// </summary>
    public interface IDateTimeBaseConverter:IValueConverter,IMultiValueConverter
    {
        /// <summary>
        /// Transforms the  converted datetime.
        /// This is used as a hook, the baseclass handles the TimeZoneConverting and the inheritor
        /// can add logic here for implementing different converters
        /// </summary>
        /// <param name="convertedDateTime">The converted date time.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        object Transform(DateTime convertedDateTime, object parameter);

        /// <summary>
        /// Transforms the converted datetime.
        /// This is used as a hook, the baseclass handles the TimeZoneConverting and the inheritor
        /// can add logic here for implementing different converters
        /// </summary>
        /// <param name="convertedDateTime">The converted date time.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        object TransformBack(DateTime convertedDateTime, object parameter);
    }
}