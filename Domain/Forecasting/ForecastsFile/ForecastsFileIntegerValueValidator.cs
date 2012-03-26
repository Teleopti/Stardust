using Teleopti.Ccc.Domain.Forecasting.Import;

namespace Teleopti.Ccc.Domain.Forecasting.ForecastsFile
{
    public class ForecastsFileIntegerValueValidator : IForecastsFileValidator<int>
    {
        public bool TryParse(string value, out ForecastParseResult<int> result)
        {
            result = new ForecastParseResult<int>();
            int parseResult;
            if (int.TryParse(value, out parseResult))
            {
                result.Success = true;
                result.Value = parseResult;
                return true;
            }
            result.ErrorMessage = string.Format("{0} should be an integer.", value);
            return false;
        }
    }
}