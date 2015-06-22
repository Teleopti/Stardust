using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class IndexVolumesLongTerm : IIndexVolumes
	{
		public IEnumerable<IVolumeYear> Create(ITaskOwnerPeriod historicalData)
		{
			return new IVolumeYear[]
			{
				new DayOfWeeks(historicalData, new DaysOfWeekCreator()),
				new WeekOfMonth(historicalData, new WeekOfMonthCreator()),
				new MonthOfYear(historicalData, new MonthOfYearCreator())
			};
		}
	}
}