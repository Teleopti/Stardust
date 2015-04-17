using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface IPreForecastWorkload
	{
		IDictionary<DateOnly, IDictionary<ForecastMethodType, double>> PreForecast(IWorkload workload, DateOnlyPeriod futurePeriod, TaskOwnerPeriod historicalDataForForecasting);
	}
}