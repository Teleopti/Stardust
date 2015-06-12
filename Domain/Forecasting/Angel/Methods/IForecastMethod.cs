using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Methods
{
	public interface IForecastMethod
	{
		ForecastResult Forecast(ITaskOwnerPeriod historicalData, DateOnlyPeriod futurePeriod);
		ForecastMethodType Id { get; }
		ForecastResult SeasonalVariation(ITaskOwnerPeriod historicalData);
	}
}