using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public class ForecastAccuracyCalculator : IForecastAccuracyCalculator
	{
		public AccuracyModel Accuracy(IDictionary<DateOnly, double> forecastedTasksForLastYear,
			IDictionary<DateOnly, TimeSpan> forecastedTaskTimeForLastYear,
			IDictionary<DateOnly, TimeSpan> forecastedAfterTaskTimeForLastYear,
			IList<ITaskOwner> historicalDataForLastYear)
		{
			var diffTasksSum = 0d;
			var diffTaskTimeSum = 0d;
			var diffAfterTaskTimeSum = 0d;
			var tasksSum = 0d;
			var taskTimeSum = 0d;
			var afterTaskTimeSum = 0d;
			foreach (var day in historicalDataForLastYear)
			{
				var diffTasks = Math.Abs(day.TotalStatisticCalculatedTasks - forecastedTasksForLastYear[day.CurrentDate]);
				tasksSum += day.TotalStatisticCalculatedTasks;
				diffTasksSum += diffTasks;

				var diffTaskTime = Math.Abs(day.TotalStatisticAverageTaskTime.TotalSeconds - forecastedTaskTimeForLastYear[day.CurrentDate].TotalSeconds);
				taskTimeSum += day.TotalStatisticAverageTaskTime.TotalSeconds;
				diffTaskTimeSum += diffTaskTime;

				var diffAfterTaskTime = Math.Abs(day.TotalStatisticAverageAfterTaskTime.TotalSeconds - forecastedAfterTaskTimeForLastYear[day.CurrentDate].TotalSeconds);
				afterTaskTimeSum += day.TotalStatisticAverageAfterTaskTime.TotalSeconds;
				diffAfterTaskTimeSum += diffAfterTaskTime;
			}
			var weightedMeanAbsolutePercentageTasksError = Math.Abs(tasksSum) < 0.000001 ? 0 : diffTasksSum / tasksSum;
			var weightedMeanAbsolutePercentageTaskTimeError = Math.Abs(taskTimeSum) < 0.000001 ? 0 : diffTaskTimeSum / taskTimeSum;
			var weightedMeanAbsolutePercentageAfterTaskTimeError = Math.Abs(afterTaskTimeSum) < 0.000001 ? 0 : diffAfterTaskTimeSum / afterTaskTimeSum;
			return new AccuracyModel()
			{
				TasksPercentageError = Math.Round(Math.Max(0, 100 - weightedMeanAbsolutePercentageTasksError * 100), 1),
				TaskTimePercentageError = Math.Round(Math.Max(0, 100 - weightedMeanAbsolutePercentageTaskTimeError * 100), 1),
				AfterTaskTimePercentageError = Math.Round(Math.Max(0, 100 - weightedMeanAbsolutePercentageAfterTaskTimeError * 100), 1),
			};
		}
	}
}