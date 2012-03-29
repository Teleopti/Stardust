using System.Globalization;
using Teleopti.Ccc.Domain.Forecasting.Import;

namespace Teleopti.Ccc.Domain.Forecasting.ForecastsFile
{
    public class ForecastsFileDoubleValueValidator : IForecastsFileValidator<double>
    {
        private static readonly CultureInfo CultureInfo = new CultureInfo("en-US");

        public bool TryParse(string value, out ForecastParseResult<double> result)
        {
            result = new ForecastParseResult<double>();
            double parseResult;
            if (double.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo, out parseResult))
            {
                result.Success = true;
                result.Value = parseResult;
                return true;
            }
            result.ErrorMessage = string.Format(CultureInfo.InvariantCulture, "{0} should be a double value.", value);
            return false;
        }
    }
}