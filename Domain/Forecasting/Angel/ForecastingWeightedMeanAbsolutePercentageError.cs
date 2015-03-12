using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class ForecastingWeightedMeanAbsolutePercentageError : IForecastingMeasurer
	{
		public double Measure(IList<IForecastingTarget> forecastingForLastYear, ReadOnlyCollection<ITaskOwner> historicalDataForLastYear)
		{
			var diffSum = 0d;
			var tasksSum = 0d;

			var first = historicalDataForLastYear.Select(x => new {x.TotalStatisticCalculatedTasks, x.CurrentDate, Tasks = 0d});
			first = first.Concat(forecastingForLastYear.Select(x => new {TotalStatisticCalculatedTasks = 0d, x.CurrentDate, x.Tasks}));
			var grouped =
				first.GroupBy(x => x.CurrentDate)
					.Select(
						x =>
							new
							{
								x.Key,
								Tasks = x.Sum(t => t.Tasks),
								TotalStatisticCalculatedTasks = x.Sum(t => t.TotalStatisticCalculatedTasks)
							});
			foreach (var day in grouped)
			{
				double diff;
				if (Math.Abs(day.TotalStatisticCalculatedTasks) < 0.000001 && Math.Abs(day.Tasks) < 0.000001)
				{
					diff = 0d;
				}
				else
				{
					diff = Math.Abs(day.TotalStatisticCalculatedTasks - day.Tasks);
					tasksSum += day.TotalStatisticCalculatedTasks;
				}
				diffSum += diff;
			}

			var wmape = Math.Abs(tasksSum) < 0.000001 ? 0 : diffSum / tasksSum;
			return Math.Max(0, 100 - Math.Round(wmape * 100, 3));
		}
	}
}