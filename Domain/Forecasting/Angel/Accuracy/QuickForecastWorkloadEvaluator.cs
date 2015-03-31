using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public class QuickForecastWorkloadEvaluator : IQuickForecastWorkloadEvaluator
	{
		private readonly IHistoricalData _historicalData;
		private readonly IForecastingMeasurer _forecastingMeasurer;
		private readonly ForecastMethodProvider _forecastMethodProvider;

		public QuickForecastWorkloadEvaluator(IHistoricalData historicalData, IForecastingMeasurer forecastingMeasurer, ForecastMethodProvider forecastMethodProvider)
		{
			_historicalData = historicalData;
			_forecastingMeasurer = forecastingMeasurer;
			_forecastMethodProvider = forecastMethodProvider;
		}

		public ForecastingAccuracy Measure(IWorkload workload, DateOnlyPeriod historicalPeriod)
		{
			var historicalData = _historicalData.Fetch(workload, historicalPeriod);
			if (!historicalData.TaskOwnerDayCollection.Any())
				return new ForecastingAccuracy {WorkloadId = workload.Id.Value, Accuracies = new List<MethodAccuracy>()};

			var oneYearBack = new DateOnly(historicalData.EndDate.Date.AddYears(-1));
			var lastYearData = new TaskOwnerPeriod(DateOnly.MinValue,
				historicalData.TaskOwnerDayCollection.Where(x => x.CurrentDate > oneYearBack), TaskOwnerPeriodType.Other);
			var yearBeforeLastYearData = new TaskOwnerPeriod(DateOnly.MinValue,
				historicalData.TaskOwnerDayCollection.Where(x => x.CurrentDate <= oneYearBack), TaskOwnerPeriodType.Other);

			if (!yearBeforeLastYearData.TaskOwnerDayCollection.Any())
				return new ForecastingAccuracy {WorkloadId = workload.Id.Value, Accuracies = new List<MethodAccuracy>()};

			var result = new ForecastingAccuracy {WorkloadId = workload.Id.Value, Accuracies = new List<MethodAccuracy>()};
			var methods = _forecastMethodProvider.All();
			foreach (var forecastMethod in methods)
			{
				var forecastingMeasureTarget = forecastMethod.Forecast(yearBeforeLastYearData,
					new DateOnlyPeriod(oneYearBack, historicalData.EndDate));
				result.Accuracies.Add(new MethodAccuracy
				{
					Number = _forecastingMeasurer.Measure(forecastingMeasureTarget, lastYearData.TaskOwnerDayCollection),
					MethodId = forecastMethod.Id
				});
			}
			return result;
		}
	}
}