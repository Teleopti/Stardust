using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting.DayInMonthIndex;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class MonthlyPatternIndexVolumes : IIndexVolumes
	{
		public IEnumerable<IVolumeYear> Create(ITaskOwnerPeriod historicalData)
		{
			return new IVolumeYear[]
			{
				new DayOfWeeks(historicalData, new DaysOfWeekCreator()),
				new WeekOfMonth(historicalData, new WeekOfMonthCreator()),
				new MonthOfYear(historicalData, new MonthOfYearCreator()),
				new DayInMonth(historicalData, new DayInMonthCreator())
			};
		}
	}
}