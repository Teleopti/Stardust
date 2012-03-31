using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Forecasting.Import;

namespace Teleopti.Ccc.Domain.Forecasting.ForecastsFile
{
    public class ForecastsFileDateTimeValidator : IForecastsFileValidator<DateTime>
    {
        public bool TryParse(string value, out ForecastParseResult<DateTime> result)
        {
            result = new ForecastParseResult<DateTime>();
            DateTime parseResult;
            if (DateTime.TryParseExact(value, "yyyyMMdd HH:mm", null, DateTimeStyles.None, out parseResult))
            {
                result.Value = parseResult;
                result.Success = true;
                return true;
            }
            result.ErrorMessage = string.Format(CultureInfo.InvariantCulture, "Date time format of {0} is wrong", value);
            return false;
        }
    }
}