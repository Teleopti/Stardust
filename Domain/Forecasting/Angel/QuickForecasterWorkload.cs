using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class QuickForecasterWorkload : IQuickForecasterWorkload
	{
		private readonly IHistoricalData _historicalData;
		private readonly IFutureData _futureData;
		private readonly IForecastMethod _forecastMethod;
		private readonly IForecastingTargetMerger _forecastingTargetMerger;

		public QuickForecasterWorkload(IHistoricalData historicalData, IFutureData futureData, IForecastMethod forecastMethod, IForecastingTargetMerger forecastingTargetMerger)
		{
			_historicalData = historicalData;
			_futureData = futureData;
			_forecastMethod = forecastMethod;
			_forecastingTargetMerger = forecastingTargetMerger;
		}

		public void Execute(QuickForecasterWorkloadParams quickForecasterWorkloadParams)
		{
			var historicalData = _historicalData.Fetch(quickForecasterWorkloadParams.WorkLoad, quickForecasterWorkloadParams.HistoricalPeriod);
			if (!historicalData.TaskOwnerDayCollection.Any())
				return;
			var forecastingTargets = _forecastMethod.Forecast(historicalData, quickForecasterWorkloadParams.FuturePeriod);
			var futureWorkloadDays = _futureData.Fetch(quickForecasterWorkloadParams);
			_forecastingTargetMerger.Merge(forecastingTargets, futureWorkloadDays);
		}
	}
}