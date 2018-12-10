using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.Domain.Forecasting.Angel.Outlier;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public class ForecastWorkloadEvaluator : IForecastWorkloadEvaluator
	{
		private readonly IHistoricalData _historicalData;
		private readonly IForecastMethodProvider _forecastMethodProvider;
		private readonly IHistoricalPeriodProvider _historicalPeriodProvider;

		public ForecastWorkloadEvaluator(IHistoricalData historicalData,
			IForecastMethodProvider forecastMethodProvider,
			IHistoricalPeriodProvider historicalPeriodProvider)
		{
			_historicalData = historicalData;
			_forecastMethodProvider = forecastMethodProvider;
			_historicalPeriodProvider = historicalPeriodProvider;
		}

		public WorkloadAccuracy Evaluate(IWorkload workload, OutlierRemover outlierRemover,
			IForecastAccuracyCalculator forecastAccuracyCalculator)
		{
			var result = new WorkloadAccuracy
			{
				ForecastMethodTypeForTasks = ForecastMethodType.None,
				ForecastMethodTypeForTaskTime = ForecastMethodType.None,
				ForecastMethodTypeForAfterTaskTime = ForecastMethodType.None,
			};

			var availablePeriod = _historicalPeriodProvider.AvailablePeriod(workload);
			if (!availablePeriod.HasValue) return result;

			var historicalData = _historicalData.Fetch(workload, availablePeriod.Value);
			if (!historicalData.TaskOwnerDayCollection.Any()) return result;

			var twoPeriods = HistoricalPeriodProvider.DivideIntoTwoPeriods(availablePeriod.Value);
			var firstPartHistoricalData =
				historicalData.TaskOwnerDayCollection.Where(x => x.CurrentDate <= twoPeriods.Item1.EndDate).ToList();
			if (!firstPartHistoricalData.Any())
			{
				return result;
			}

			var secondPartHistoricalData =
				historicalData.TaskOwnerDayCollection.Where(x => x.CurrentDate > twoPeriods.Item1.EndDate).ToList();

			var methods = _forecastMethodProvider.Calculate(availablePeriod.Value);
			var methodsEvaluationResult = new List<MethodAccuracy>();
			foreach (var forecastMethod in methods)
			{
				var firstPeriodData = new TaskOwnerPeriod(DateOnly.MinValue,
					firstPartHistoricalData.Select(taskOwner => cloneTaskOwner(workload, taskOwner)),
					TaskOwnerPeriodType.Other);
				
				var firstPeriodDataNoOutliers = outlierRemover.RemoveOutliers(firstPeriodData, forecastMethod, forecastMethod, forecastMethod);

				foreach (var taskOwner in firstPeriodData.TaskOwnerDayCollection)
				{
					if (firstPeriodDataNoOutliers.Tasks.ContainsKey(taskOwner.CurrentDate))
						((ValidatedVolumeDay) taskOwner).ValidatedTasks =
							firstPeriodDataNoOutliers.Tasks[taskOwner.CurrentDate];

					if (firstPeriodDataNoOutliers.TaskTime.ContainsKey(taskOwner.CurrentDate))
						((ValidatedVolumeDay) taskOwner).ValidatedAverageTaskTime =
						TimeSpan.FromSeconds(firstPeriodDataNoOutliers.TaskTime[taskOwner.CurrentDate]);

					if (firstPeriodDataNoOutliers.AfterTaskTime.ContainsKey(taskOwner.CurrentDate))
						((ValidatedVolumeDay) taskOwner).ValidatedAverageAfterTaskTime =
							TimeSpan.FromSeconds(firstPeriodDataNoOutliers.AfterTaskTime[taskOwner.CurrentDate]);
				}


				var forecastTasks = forecastMethod.ForecastTasks(firstPeriodData, twoPeriods.Item2);
				var forecastTaskTime = forecastMethod.ForecastTaskTime(firstPeriodData, twoPeriods.Item2);
				var forecastAfterTaskTime = forecastMethod.ForecastAfterTaskTime(firstPeriodData, twoPeriods.Item2);

				var accuracyModel = forecastAccuracyCalculator.Accuracy(forecastTasks, forecastTaskTime, forecastAfterTaskTime,
					secondPartHistoricalData);
				methodsEvaluationResult.Add(new MethodAccuracy
				{
					NumberForTask = accuracyModel.TasksPercentageError,
					NumberForTaskTime = accuracyModel.TaskTimePercentageError,
					NumberForAfterTaskTime = accuracyModel.AfterTaskTimePercentageError,
					MethodId = forecastMethod.Id
				});
			}

			result.ForecastMethodTypeForTasks = methodsEvaluationResult.OrderByDescending(x => x.NumberForTask).First().MethodId;
			result.ForecastMethodTypeForTaskTime = methodsEvaluationResult.OrderByDescending(x => x.NumberForTaskTime).First().MethodId;
			result.ForecastMethodTypeForAfterTaskTime = methodsEvaluationResult.OrderByDescending(x => x.NumberForAfterTaskTime).First().MethodId;
			return result;
		}

		private IValidatedVolumeDay cloneTaskOwner(IWorkload workload, ITaskOwner taskOwner)
		{
			var validatedVolumeDay = (IValidatedVolumeDay) taskOwner;
			return new ValidatedVolumeDay(workload, taskOwner.CurrentDate)
			{
				ValidatedTasks = validatedVolumeDay.ValidatedTasks,
				ValidatedAverageAfterTaskTime = validatedVolumeDay.ValidatedAverageAfterTaskTime,
				ValidatedAverageTaskTime = validatedVolumeDay.ValidatedAverageTaskTime,
				TaskOwner = validatedVolumeDay.TaskOwner
			};
		}
	}
}