using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Methods
{
	public interface IForecastMethod
	{
		ForecastMethodResult Forecast(ITaskOwnerPeriod historicalData, DateOnlyPeriod futurePeriod);
		ForecastMethodType Id { get; }
		IEnumerable<DateAndTask> SeasonalVariation(ITaskOwnerPeriod historicalData);
	}
}