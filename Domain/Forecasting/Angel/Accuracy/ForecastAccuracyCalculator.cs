using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public class ForecastAccuracyCalculator : IForecastAccuracyCalculator
	{
		public AccuracyModel Accuracy(IDictionary<DateOnly, double> forecastedTasksForLastYear,
			IDictionary<DateOnly, TimeSpan> forecastedTaskTimeForLastYear,
			ReadOnlyCollection<ITaskOwner> historicalDataForLastYear)
		{
			var diffTasksSum = 0d;
			var diffTaskTimeSum = 0d;
			var tasksSum = 0d;
			var taskTimeSum = 0d;
			foreach (var day in historicalDataForLastYear)
			{
				var diffTasks = Math.Abs(day.TotalStatisticCalculatedTasks - forecastedTasksForLastYear[day.CurrentDate]);
				tasksSum += day.TotalStatisticCalculatedTasks;
				diffTasksSum += diffTasks;

				var diffTaskTime = Math.Abs(day.TotalStatisticAverageTaskTime.TotalSeconds - forecastedTaskTimeForLastYear[day.CurrentDate].TotalSeconds);
				taskTimeSum += day.TotalStatisticAverageTaskTime.TotalSeconds;
				diffTaskTimeSum += diffTaskTime;
			}
			var weightedMeanAbsolutePercentageTasksError = Math.Abs(tasksSum) < 0.000001 ? 0 : diffTasksSum / tasksSum;
			var weightedMeanAbsolutePercentageTaskTimeError = Math.Abs(taskTimeSum) < 0.000001 ? 0 : diffTaskTimeSum / taskTimeSum;
			return new AccuracyModel()
			{
				TasksPercentageError = Math.Round(Math.Max(0, 100 - weightedMeanAbsolutePercentageTasksError * 100), 1),
				TaskTimePercentageError = Math.Round(Math.Max(0, 100 - weightedMeanAbsolutePercentageTaskTimeError * 100), 1),

			};
		}
	}
}