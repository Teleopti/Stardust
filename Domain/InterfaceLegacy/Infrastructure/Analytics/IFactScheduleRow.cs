namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics
{
	public interface IFactScheduleRow
	{
		IAnalyticsFactScheduleDate DatePart { get; set; }
		IAnalyticsFactScheduleTime TimePart { get; set; }
		IAnalyticsFactSchedulePerson PersonPart { get; set; }
	}
}