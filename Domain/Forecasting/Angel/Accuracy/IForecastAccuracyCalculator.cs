using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public interface IForecastAccuracyCalculator
	{
		AccuracyModel Accuracy(IDictionary<DateOnly, double> forecastedTasksForLastYear,
			IDictionary<DateOnly, TimeSpan> forecastedTaskTimeForLastYear,
			ReadOnlyCollection<ITaskOwner> historicalDataForLastYear);
	}
}