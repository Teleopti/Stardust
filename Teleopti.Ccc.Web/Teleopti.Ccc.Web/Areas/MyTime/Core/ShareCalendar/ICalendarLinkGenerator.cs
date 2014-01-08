namespace Teleopti.Ccc.Web.Areas.MyTime.Core.ShareCalendar
{
	public interface ICalendarLinkGenerator
	{
		string Generate(CalendarLinkId calendarLinkId);
	}
}