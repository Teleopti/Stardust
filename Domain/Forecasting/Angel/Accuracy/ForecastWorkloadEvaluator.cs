using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.Domain.Forecasting.Angel.Outlier;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public class ForecastWorkloadEvaluator : IForecastWorkloadEvaluator
	{
		private readonly IHistoricalData _historicalData;
		private readonly IForecastAccuracyCalculator _forecastAccuracyCalculator;
		private readonly IForecastMethodProvider _forecastMethodProvider;
		private readonly IHistoricalPeriodProvider _historicalPeriodProvider;
		private readonly IOutlierRemover _outlierRemover;

		public ForecastWorkloadEvaluator(IHistoricalData historicalData, IForecastAccuracyCalculator forecastAccuracyCalculator, IForecastMethodProvider forecastMethodProvider, IHistoricalPeriodProvider historicalPeriodProvider, IOutlierRemover outlierRemover)
		{
			_historicalData = historicalData;
			_forecastAccuracyCalculator = forecastAccuracyCalculator;
			_forecastMethodProvider = forecastMethodProvider;
			_historicalPeriodProvider = historicalPeriodProvider;
			_outlierRemover = outlierRemover;
		}

		public WorkloadAccuracy Evaluate(IWorkload workload)
		{
			var result = new WorkloadAccuracy { Id = workload.Id.Value, Name = workload.Name};
			var methods = _forecastMethodProvider.All();
			var methodsEvaluationResult = new List<MethodAccuracy>();
			foreach (var forecastMethod in methods)
			{
				var availablePeriod = _historicalPeriodProvider.AvailablePeriod(workload);
				if(!availablePeriod.HasValue)
					return new WorkloadAccuracy { Id = workload.Id.Value, Name = workload.Name, Accuracies = new MethodAccuracy[] { } };
				var historicalData = _historicalData.Fetch(workload, availablePeriod.Value);
				if (!historicalData.TaskOwnerDayCollection.Any())
					return new WorkloadAccuracy { Id = workload.Id.Value, Name = workload.Name, Accuracies = new MethodAccuracy[] { } };

				var twoPeriods = HistoricalPeriodProvider.DivideIntoTwoPeriods(availablePeriod.Value);
				var firstPeriodData = new TaskOwnerPeriod(DateOnly.MinValue, historicalData.TaskOwnerDayCollection.Where(x => x.CurrentDate <= twoPeriods.Item1.EndDate), TaskOwnerPeriodType.Other);
				var secondPeriodData = new TaskOwnerPeriod(DateOnly.MinValue, historicalData.TaskOwnerDayCollection.Where(x => x.CurrentDate > twoPeriods.Item1.EndDate), TaskOwnerPeriodType.Other);

				if (!firstPeriodData.TaskOwnerDayCollection.Any())
					return new WorkloadAccuracy { Id = workload.Id.Value, Name = workload.Name, Accuracies = new MethodAccuracy[] { } };

				var firstPeriodDataNoOutliers = _outlierRemover.RemoveOutliers(firstPeriodData, forecastMethod);
				var forecastResult = forecastMethod.Forecast(firstPeriodDataNoOutliers, twoPeriods.Item2);

				methodsEvaluationResult.Add(new MethodAccuracy
				{
					MeasureResult = forecastResult.ForecastingTargets.ToArray(),
					Number = _forecastAccuracyCalculator.Accuracy(forecastResult.ForecastingTargets, secondPeriodData.TaskOwnerDayCollection),
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
	}
}