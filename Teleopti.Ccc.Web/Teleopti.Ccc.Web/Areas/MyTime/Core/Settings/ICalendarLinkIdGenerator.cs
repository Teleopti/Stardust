namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Settings
{
	public interface ICalendarLinkIdGenerator
	{
		string Generate();
		CalendarLinkId Parse(string id);
	}
}