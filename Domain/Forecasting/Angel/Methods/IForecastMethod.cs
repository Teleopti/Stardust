using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Methods
{
	public interface IForecastMethod
	{
		IDictionary<DateOnly, double> ForecastTasks(ITaskOwnerPeriod historicalData, DateOnlyPeriod futurePeriod);
		IDictionary<DateOnly, TimeSpan> ForecastTaskTime(ITaskOwnerPeriod historicalData, DateOnlyPeriod futurePeriod);
		ForecastMethodType Id { get; }
		Dictionary<DateOnly, double> SeasonalVariationTasks(ITaskOwnerPeriod historicalData);
		Dictionary<DateOnly, double> SeasonalVariationTaskTime(ITaskOwnerPeriod historicalData);
	}
}