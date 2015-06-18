using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class DayWeekMonthIndexVolumes : IDayWeekMonthIndexVolumes
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

	public interface IDayWeekMonthIndexVolumes: IIndexVolumes
	{
	}
}