using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class ForecastingWeightedMeanAbsolutePercentageError : IForecastingMeasurer
	{
		public double Measure(IList<IForecastingTarget> forecastingForLastYear, ReadOnlyCollection<ITaskOwner> historicalDataForLastYear)
		{
			var diffSum = 0d;
			var numberOfSkipped = 0;
			var tasksSum = 0d;
			foreach (var day in historicalDataForLastYear)
			{
				var diff = 0d;
				foreach (var forecastingDay in forecastingForLastYear)
				{
					if (forecastingDay.CurrentDate == day.CurrentDate)
					{
						if (Math.Abs(day.TotalStatisticCalculatedTasks) < 0.000001 && Math.Abs(forecastingDay.Tasks) < 0.000001)
						{
							diff = 0d;
						}
						else if (Math.Abs(day.TotalStatisticCalculatedTasks) < 0.000001)
						{
							// a main drawback in this measurement method
							numberOfSkipped++;
						}
						else
						{
							diff = Math.Abs(day.TotalStatisticCalculatedTasks - forecastingDay.Tasks);
							tasksSum += day.TotalStatisticCalculatedTasks;
						}
						break;
					}
				}

				diffSum += diff;
			}
			if (historicalDataForLastYear.Count - numberOfSkipped == 0)
			{
				return Double.NaN;
			}
			var wmape = Math.Abs(tasksSum) < 0.000001 ? 0 : diffSum / tasksSum;
			return Math.Max(0, 100 - Math.Round(wmape / (historicalDataForLastYear.Count - numberOfSkipped) * 100, 3));
		}
	}
}