using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class QuickForecasterWorkload : IQuickForecasterWorkload
	{
		private readonly IHistoricalData _historicalData;
		private readonly IFutureData _futureData;
		private readonly IForecastVolumeApplier _volumeApplier;

		public QuickForecasterWorkload(IHistoricalData historicalData, IFutureData futureData, IForecastVolumeApplier volumeApplier)
		{
			_historicalData = historicalData;
			_futureData = futureData;
			_volumeApplier = volumeApplier;
		}

		public void Execute(QuickForecasterWorkloadParams quickForecasterWorkloadParams)
		{
			var historicalData = _historicalData.Fetch(quickForecasterWorkloadParams.WorkLoad, quickForecasterWorkloadParams.HistoricalPeriod);

			var futureWorkloadDays = _futureData.Fetch(quickForecasterWorkloadParams);

			_volumeApplier.Apply(quickForecasterWorkloadParams.WorkLoad, historicalData, futureWorkloadDays);

			quickForecasterWorkloadParams.Accuracy = 0;
		}
	}
}