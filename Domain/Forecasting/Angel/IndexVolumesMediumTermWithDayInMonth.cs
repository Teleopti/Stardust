using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting.DayInMonthIndex;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class IndexVolumesMediumTermWithDayInMonth : IIndexVolumes
	{
		public IEnumerable<IVolumeYear> Create(ITaskOwnerPeriod historicalData)
		{
			return new IVolumeYear[]
			{
				new DayOfWeeks(historicalData, new DaysOfWeekCreator()),
				new WeekOfMonth(historicalData, new WeekOfMonthCreator()),
				new DayInMonth(historicalData, new DayInMonthCreator())
			};
		}
	}
}