using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider
{
	public interface ICalendarLinkSettingsPersisterAndProvider
	{
		CalendarLinkSettings Persist(CalendarLinkSettings isActive);
		CalendarLinkSettings Get();
		CalendarLinkSettings GetByOwner(IPerson person);
	}
}