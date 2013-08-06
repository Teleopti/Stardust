using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider
{
	public interface ICalendarLinkSettingsPersisterAndProvider
	{
		CalendarLinkSettings Persist(CalendarLinkSettings isActive);
		CalendarLinkSettings Get();
	}
}