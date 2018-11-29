using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public interface IForecastAccuracyCalculator
	{
		AccuracyModel Accuracy(IDictionary<DateOnly, double> forecastedTasksForLastYear,
			IDictionary<DateOnly, TimeSpan> forecastedTaskTimeForLastYear,
			IDictionary<DateOnly, TimeSpan> forecastedAfterTaskTimeForLastYear,
			IList<ITaskOwner> historicalDataForLastYear);
	}
}