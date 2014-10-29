using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Interfaces.Domain;

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

		public void Execute(IWorkload workload, DateOnlyPeriod historicalPeriod, DateOnlyPeriod futurePeriod)
		{
			var taskOwnerPeriod = _historicalData.Fetch(workload, historicalPeriod);
			var futureWorkloadDays = _futureData.Fetch(workload, futurePeriod);
			_volumeApplier.Apply(workload, taskOwnerPeriod, futureWorkloadDays);
		}
	}
}