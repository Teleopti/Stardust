using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public class QuickForecastWorkloadEvaluator : IQuickForecastWorkloadEvaluator
	{
		private readonly IHistoricalData _historicalData;
		private readonly IForecastMethod _forecastMethod;
		private readonly IForecastingMeasurer _forecastingMeasurer;

		public QuickForecastWorkloadEvaluator(IHistoricalData historicalData, IForecastMethod forecastMethod, IForecastingMeasurer forecastingMeasurer)
		{
			_historicalData = historicalData;
			_forecastMethod = forecastMethod;
			_forecastingMeasurer = forecastingMeasurer;
		}

		public ForecastingAccuracy Measure(IWorkload workload, DateOnlyPeriod historicalPeriod)
		{
			var historicalData = _historicalData.Fetch(workload, historicalPeriod);
			if (!historicalData.TaskOwnerDayCollection.Any())
				return new ForecastingAccuracy {WorkloadId = workload.Id.Value, CanForecast = false, Accuracy = double.NaN};

			var oneYearBack = new DateOnly(historicalData.EndDate.Date.AddYears(-1));
			var lastYearData = new TaskOwnerPeriod(DateOnly.MinValue, historicalData.TaskOwnerDayCollection.Where(x => x.CurrentDate > oneYearBack), TaskOwnerPeriodType.Other);
			var yearBeforeLastYearData = new TaskOwnerPeriod(DateOnly.MinValue, historicalData.TaskOwnerDayCollection.Where(x => x.CurrentDate <= oneYearBack), TaskOwnerPeriodType.Other);

			if (!yearBeforeLastYearData.TaskOwnerDayCollection.Any())
				return new ForecastingAccuracy { WorkloadId = workload.Id.Value, CanForecast = true, Accuracy = double.NaN };

			var forecastingMeasureTarget = _forecastMethod.Forecast(yearBeforeLastYearData, new DateOnlyPeriod(oneYearBack, historicalData.EndDate));
			return new ForecastingAccuracy { Accuracy = _forecastingMeasurer.Measure(forecastingMeasureTarget, lastYearData.TaskOwnerDayCollection), WorkloadId = workload.Id.Value, CanForecast = true};
		}
	}
}