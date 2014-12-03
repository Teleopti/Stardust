namespace Teleopti.Interfaces.Infrastructure
{
	public interface IAnalyticsScheduleRepository
	{
		void PersistFactScheduleRow(AnalyticsFactScheduleTime analyticsFactScheduleTime, AnalyticsFactScheduleDate analyticsFactScheduleDate, AnalyticsFactSchedulePerson personPart);
		void PersistFactScheduleDayCountRow(AnalyticsFactScheduleDayCount dayCount);
	}

	public class AnalyticsFactSchedulePerson
	{
	}

	public class AnalyticsFactScheduleDate
	{
	}


	public class AnalyticsFactScheduleTime
	{
	}
	public class AnalyticsFactScheduleDayCount
	{
	}
}