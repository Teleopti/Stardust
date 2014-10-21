using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class QuickForecaster : IQuickForecaster
	{
		private readonly IHistoricalData _historicalData;
		private readonly IFutureData _futureData;

		public QuickForecaster(IHistoricalData historicalData, IFutureData futureData)
		{
			_historicalData = historicalData;
			_futureData = futureData;
		}

		public void Execute(IWorkload workload, DateOnlyPeriod historicalPeriod, DateOnlyPeriod futurePeriod)
		{
			var taskOwnerPeriod = _historicalData.Fetch(workload, historicalPeriod);

			var futureWorkloadDays = _futureData.Fetch(workload, futurePeriod);

			//apply stuff -> move out to service(s)
			var totalVolume = new TotalVolume();
			VolumeYear volumeMonthYear = new MonthOfYear(taskOwnerPeriod, new MonthOfYearCreator());
			VolumeYear volumeWeekYear = new WeekOfMonth(taskOwnerPeriod, new WeekOfMonthCreator());
			VolumeYear volumeDayYear = new DayOfWeeks(taskOwnerPeriod, new DaysOfWeekCreator());
			var indexes = new List<IVolumeYear> {volumeMonthYear, volumeWeekYear, volumeDayYear};
			totalVolume.Create(taskOwnerPeriod, futureWorkloadDays, indexes, new IOutlier[] { }, 0, 0, false, workload);
		}
	}
}