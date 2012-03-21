using System;
using System.Globalization;

namespace Teleopti.Ccc.Domain.Forecasting.ForecastsFile
{
    public class ForecastsFileDateTimeValidator : IForecastsFileValidator
    {
        public bool Validate(string value)
        {
            try
            {
                DateTime result;
                if (DateTime.TryParseExact(value, "yyyyMMdd HH:mm", null, DateTimeStyles.None, out result))
                    return true;
                ErrorMessage = string.Format("Datetime format of {0} is wrong", value);
                return false;
            }
            catch (ArgumentException)
            {
                ErrorMessage = string.Format("Datetime format of {0} is wrong", value);
                return false;
            }
        }

        public string ErrorMessage { get; set; }
    }
}