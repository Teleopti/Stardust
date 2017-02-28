namespace Teleopti.Interfaces.Infrastructure.Analytics
{
	public interface IFactScheduleRow
	{
		IAnalyticsFactScheduleDate DatePart { get; set; }
		IAnalyticsFactScheduleTime TimePart { get; set; }
		IAnalyticsFactSchedulePerson PersonPart { get; set; }
	}
}