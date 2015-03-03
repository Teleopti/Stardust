using System;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class QuickForecasterWorkload : IQuickForecasterWorkload
	{
		private readonly IHistoricalData _historicalData;
		private readonly IFutureData _futureData;
		private readonly IForecastMethod _forecastMethod;
		private readonly IForecastingTargetMerger _forecastingTargetMerger;
		private readonly IForecastingMeasurer _forecastingMeasurer;

		public QuickForecasterWorkload(IHistoricalData historicalData, IFutureData futureData, IForecastMethod forecastMethod, IForecastingTargetMerger forecastingTargetMerger, IForecastingMeasurer forecastingMeasurer)
		{
			_historicalData = historicalData;
			_futureData = futureData;
			_forecastMethod = forecastMethod;
			_forecastingTargetMerger = forecastingTargetMerger;
			_forecastingMeasurer = forecastingMeasurer;
		}

		public void Execute(QuickForecasterWorkloadParams quickForecasterWorkloadParams)
		{
			var historicalData = _historicalData.Fetch(quickForecasterWorkloadParams.WorkLoad, quickForecasterWorkloadParams.HistoricalPeriod);
			if (!historicalData.TaskOwnerDayCollection.Any())
			{
				quickForecasterWorkloadParams.Difference = Double.NaN;
				return ;
			}

			var oneYearBack = new DateOnly(historicalData.EndDate.Date.AddYears(-1));
			var lastYearData = new TaskOwnerPeriod(DateOnly.MinValue, historicalData.TaskOwnerDayCollection.Where(x => x.CurrentDate > oneYearBack), TaskOwnerPeriodType.Other);
			var yearBeforeLastYearData = new TaskOwnerPeriod(DateOnly.MinValue, historicalData.TaskOwnerDayCollection.Where(x => x.CurrentDate <= oneYearBack), TaskOwnerPeriodType.Other);

			var forecastingTargets = _forecastMethod.Forecast(lastYearData, quickForecasterWorkloadParams.FuturePeriod);
			var futureWorkloadDays = _futureData.Fetch(quickForecasterWorkloadParams);
			_forecastingTargetMerger.Merge(forecastingTargets, futureWorkloadDays);

			if (yearBeforeLastYearData.TaskOwnerDayCollection.Any())
			{
				var forecastingMeasureTarget = _forecastMethod.Forecast(yearBeforeLastYearData,
					new DateOnlyPeriod(oneYearBack, historicalData.EndDate));
				quickForecasterWorkloadParams.Difference = _forecastingMeasurer.Measure(forecastingMeasureTarget, lastYearData.TaskOwnerDayCollection);
			}
			else
			{
				quickForecasterWorkloadParams.Difference = Double.NaN;
			}
		}
	}
}