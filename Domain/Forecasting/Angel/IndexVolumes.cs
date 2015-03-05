using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class IndexVolumes : IIndexVolumes
	{
		public IEnumerable<IVolumeYear> Create(ITaskOwnerPeriod historicalData)
		{
			return new IVolumeYear[]
			{
				new MonthOfYear(historicalData, new MonthOfYearCreator()),
				new WeekOfMonth(historicalData, new WeekOfMonthCreator()),
				new DayOfWeeks(historicalData, new DaysOfWeekCreator())
			};
		}
	}
}