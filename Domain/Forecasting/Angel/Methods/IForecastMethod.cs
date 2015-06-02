using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Methods
{
	public interface IForecastMethod
	{
		ForecastResult Forecast(TaskOwnerPeriod historicalData, DateOnlyPeriod futurePeriod, bool removeOutliers);
		ForecastMethodType Id { get; }
	}
}