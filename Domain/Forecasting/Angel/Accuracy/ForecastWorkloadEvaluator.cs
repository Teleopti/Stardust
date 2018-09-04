using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.Domain.Forecasting.Angel.Outlier;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

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

		public WorkloadAccuracy Evaluate(IWorkload workload, IOutlierRemover outlierRemover,
			IForecastAccuracyCalculator forecastAccuracyCalculator)
		{
			var result = new WorkloadAccuracy
			{
				Id = workload.Id.Value,
				Name = workload.Name,
				Accuracies = new MethodAccuracy[] { }
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
				var secondPeriodData = new TaskOwnerPeriod(DateOnly.MinValue,
					secondPartHistoricalData.Select(taskOwner => cloneTaskOwner(workload, taskOwner)),
					TaskOwnerPeriodType.Other);

				var firstPeriodDataNoOutliers = outlierRemover.RemoveOutliers(firstPeriodData, forecastMethod);
				var forecastTasks = forecastMethod.ForecastTasks(firstPeriodDataNoOutliers, twoPeriods.Item2);

				var forecastTaskTime = forecastMethod.ForecastTaskTime(firstPeriodDataNoOutliers, twoPeriods.Item2);

				var accuracyModel = forecastAccuracyCalculator.Accuracy(forecastTasks, forecastTaskTime,
					secondPeriodData.TaskOwnerDayCollection);
				methodsEvaluationResult.Add(new MethodAccuracy
				{
					Number = accuracyModel.TasksPercentageError,
					MethodId = forecastMethod.Id,
					PeriodEvaluateOn = twoPeriods.Item2,
					PeriodUsedToEvaluate = twoPeriods.Item1
				});
			}

			var bestMethod = methodsEvaluationResult.OrderByDescending(x => x.Number).First();
			bestMethod.IsSelected = true;

			result.Accuracies = methodsEvaluationResult.ToArray();
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