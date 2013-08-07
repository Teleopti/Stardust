namespace Teleopti.Ccc.Web.Areas.MyTime.Core.ShareCalendar
{
	public interface ICalendarLinkIdGenerator
	{
		string Generate();
		CalendarLinkId Parse(string id);
	}
}