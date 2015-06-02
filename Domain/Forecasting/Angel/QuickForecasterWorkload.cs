﻿using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.Domain.Forecasting.Angel.Outlier;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class QuickForecasterWorkload : IQuickForecasterWorkload
	{
		private readonly IHistoricalData _historicalData;
		private readonly IFutureData _futureData;
		private readonly IForecastMethodProvider _forecastMethodProvider;
		private readonly IForecastingTargetMerger _forecastingTargetMerger;
		private readonly IOutlierRemover _outlierRemover;

		public QuickForecasterWorkload(IHistoricalData historicalData, IFutureData futureData, IForecastMethodProvider forecastMethodProvider, IForecastingTargetMerger forecastingTargetMerger, IOutlierRemover outlierRemover)
		{
			_historicalData = historicalData;
			_futureData = futureData;
			_forecastMethodProvider = forecastMethodProvider;
			_forecastingTargetMerger = forecastingTargetMerger;
			_outlierRemover = outlierRemover;
		}

		public void Execute(QuickForecasterWorkloadParams quickForecasterWorkloadParams)
		{
			var historicalData = _historicalData.Fetch(quickForecasterWorkloadParams.WorkLoad, quickForecasterWorkloadParams.HistoricalPeriod);
			if (!historicalData.TaskOwnerDayCollection.Any())
				return;
			var forecastMethod = _forecastMethodProvider.Get(quickForecasterWorkloadParams.ForecastMethodId);
			var historicalDataNoOutliers = _outlierRemover.RemoveOutliers(historicalData, forecastMethod);
			var forecastResult = forecastMethod.Forecast(historicalDataNoOutliers, quickForecasterWorkloadParams.FuturePeriod);
			var futureWorkloadDays = _futureData.Fetch(quickForecasterWorkloadParams);
			_forecastingTargetMerger.Merge(forecastResult.ForecastingTargets, futureWorkloadDays);
		}
	}
}