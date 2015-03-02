using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Teleopti.Ccc.Domain.Common.Time;
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
		private readonly BlackBox _blackBox;

		public QuickForecasterWorkload(IHistoricalData historicalData, IFutureData futureData, IForecastVolumeApplier volumeApplier, BlackBox blackBox)
		{
			_historicalData = historicalData;
			_futureData = futureData;
			_volumeApplier = volumeApplier;
			_blackBox = blackBox;
		}

		public void Execute(QuickForecasterWorkloadParams quickForecasterWorkloadParams)
		{
			var historicalData = _historicalData.Fetch(quickForecasterWorkloadParams.WorkLoad, quickForecasterWorkloadParams.HistoricalPeriod);

			var oneYearBack = new DateOnly(historicalData.EndDate.Date.AddYears(-1));
			var oneYearBackData = new TaskOwnerPeriod();

			var forecastingTarget = _blackBox.ForecastingTarget(historicalData.TaskOwnerDayCollection.Where(x => x.CurrentDate > oneYearBack), quickForecasterWorkloadParams.FuturePeriod);
			var forecastingTarget = _blackBox.ForecastingTarget(historicalData, quickForecasterWorkloadParams.FuturePeriod);
			var forecastingMeasureTarget = _blackBox.ForecastingTarget(historicalData, quickForecasterWorkloadParams.FuturePeriod);

			var futureWorkloadDays = _futureData.Fetch(quickForecasterWorkloadParams);

			_volumeApplier.Apply(quickForecasterWorkloadParams.WorkLoad, historicalData, futureWorkloadDays);

			quickForecasterWorkloadParams.Accuracy = 0;
		}
	}

	public class BlackBox : IBlackBox
	{
		public IList<IForecastingTarget> ForecastingTarget(TaskOwnerPeriod historicalData, DateOnlyPeriod futurePeriod)
		{
			return new List<IForecastingTarget>();
		}
	}

	public interface IBlackBox
	{
		IList<IForecastingTarget> ForecastingTarget(TaskOwnerPeriod historicalData, DateOnlyPeriod futurePeriod);
	}
}