using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface IForecastMethod
	{
		IList<IForecastingTarget> Forecast(TaskOwnerPeriod historicalData, DateOnlyPeriod futurePeriod);
		ForecastMethodType Id { get; }
	}
}