using System;
using System.Windows.Data;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Win.WpfControls.Converters
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    class ILayerConverter:IMultiValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// Temporary converter, Checks layer for databinding  [0] Checks for Absencelayer, [1] for togglebutton
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-02-27
        /// </remarks>
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //Possible to revert
            string isRev = parameter as string;
            if (isRev!=null)
            {
                if (isRev == "Reverse")
                {
                    if ((values[0] as AbsenceLayer) == null) return true;
                    if ((values[1] as bool?) != null) return !(bool)values[1];
                    return true;
                
                }
              
            
            }
            if ((values[0] as AbsenceLayer) == null) return false;
            if ((values[1] as bool?)!=null) return (bool)values[1];
            return false;
                   
               

              
        }

        /// <summary>
        /// Convertback Not Implemented
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetTypes">The target types.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns></returns>
        /// <remarks>
        /// Not implemented
        /// Created by: henrika
        /// Created date: 2008-02-27
        /// </remarks>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
