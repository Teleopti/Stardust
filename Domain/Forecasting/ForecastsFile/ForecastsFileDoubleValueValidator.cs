using System.Globalization;
using Teleopti.Ccc.Domain.Forecasting.Import;

namespace Teleopti.Ccc.Domain.Forecasting.ForecastsFile
{
    public class ForecastsFileDoubleValueValidator : IForecastsFileValidator<double>
    {
        private static readonly CultureInfo _cultureInfo = new CultureInfo("en-US");

        public bool TryParse(string value, out ForecastParseResult<double> result)
        {
            result = new ForecastParseResult<double>();
            double parseResult;
            if (double.TryParse(value, NumberStyles.AllowDecimalPoint, _cultureInfo, out parseResult))
            {
                result.Success = true;
                result.Value = parseResult;
                return true;
            }
            result.ErrorMessage = string.Format("{0} should be a double value.", value);
            return false;
        }
    }
}