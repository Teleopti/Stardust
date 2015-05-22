using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public class ForecastingWeightedMeanAbsolutePercentageError : IForecastMethodEvaluator
	{
		public double Evaluate(IList<IForecastingTarget> forecastingForLastYear, ReadOnlyCollection<ITaskOwner> historicalDataForLastYear)
		{
			var diffSum = 0d;
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
			var wmape = Math.Abs(tasksSum) < 0.000001 ? 0 : diffSum / tasksSum;
			return Math.Round(Math.Max(0, 100 - wmape * 100), 1);
		}
	}
}