using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Forecasting.Import;

namespace Teleopti.Ccc.Domain.Forecasting.ForecastsFile
{
    public class ForecastsFileDateTimeUnifiedValidator : IForecastsFileValidator<DateTime>
    {
        public bool TryParse(string value, out ForecastParseResult<DateTime> result)
        {
            result = new ForecastParseResult<DateTime>();
           // DateTime parseResult;
			
			var formats = new[]
				{
					"yyyyMMdd H:mm", "yyyy-MM-dd H:mm"
				};
			
			if(DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parseResult))
			{
				result.Value = parseResult;
				result.Success = true;
				return true;
			}
			
            result.ErrorMessage = $"Date time format of {value} is wrong. Supported formats are: {string.Join(",",formats)}";
            return false;
        }
    }
}