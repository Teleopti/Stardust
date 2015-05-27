using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public class ForecastWorkloadEvaluator : IForecastWorkloadEvaluator
	{
		private readonly IHistoricalData _historicalData;
		private readonly IForecastAccuracyCalculator _forecastAccuracyCalculator;
		private readonly IForecastMethodProvider _forecastMethodProvider;
		private readonly IHistoricalPeriodProvider _historicalPeriodProvider;

		public ForecastWorkloadEvaluator(IHistoricalData historicalData, IForecastAccuracyCalculator forecastAccuracyCalculator, IForecastMethodProvider forecastMethodProvider, IHistoricalPeriodProvider historicalPeriodProvider)
		{
			_historicalData = historicalData;
			_forecastAccuracyCalculator = forecastAccuracyCalculator;
			_forecastMethodProvider = forecastMethodProvider;
			_historicalPeriodProvider = historicalPeriodProvider;
		}

		public WorkloadAccuracy Evaluate(IWorkload workload)
		{
			var historicalData = _historicalData.Fetch(workload, _historicalPeriodProvider.PeriodForForecast(workload));
			if (!historicalData.TaskOwnerDayCollection.Any())
				return new WorkloadAccuracy { Id = workload.Id.Value, Name = workload.Name, Accuracies = new MethodAccuracy[] { } };

			var oneYearBack = new DateOnly(historicalData.EndDate.Date.AddYears(-1));
			var lastYearData = new TaskOwnerPeriod(DateOnly.MinValue,
				historicalData.TaskOwnerDayCollection.Where(x => x.CurrentDate > oneYearBack), TaskOwnerPeriodType.Other);
			var yearsBeforeLastYearData = new TaskOwnerPeriod(DateOnly.MinValue,
				historicalData.TaskOwnerDayCollection.Where(x => x.CurrentDate <= oneYearBack), TaskOwnerPeriodType.Other);

			if (!yearsBeforeLastYearData.TaskOwnerDayCollection.Any())
				return new WorkloadAccuracy { Id = workload.Id.Value, Name = workload.Name, Accuracies = new MethodAccuracy[] { } };

			var result = new WorkloadAccuracy { Id = workload.Id.Value, Name = workload.Name};
			var methods = _forecastMethodProvider.All();
			var list = (from forecastMethod in methods
				let forecastingMeasureTarget =
					forecastMethod.Forecast(yearsBeforeLastYearData, new DateOnlyPeriod(oneYearBack, historicalData.EndDate))
				select new MethodAccuracy
				{
					MeasureResult = forecastingMeasureTarget.ToArray(),
					Number = _forecastAccuracyCalculator.Accuracy(forecastingMeasureTarget, lastYearData.TaskOwnerDayCollection),
					MethodId = forecastMethod.Id
				}).ToList();
			var bestMethod = list.OrderByDescending(x => x.Number).First();
			bestMethod.IsSelected = true;

			result.Accuracies = list.ToArray();
			return result;
		}
	}
}