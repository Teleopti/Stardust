using Teleopti.Ccc.Domain.Forecasting.Import;

namespace Teleopti.Ccc.Domain.Forecasting.ForecastsFile
{
    public interface IForecastsFileValidator<T>
    {
        bool TryParse(string value,out ForecastParseResult<T> result);
    }
}
