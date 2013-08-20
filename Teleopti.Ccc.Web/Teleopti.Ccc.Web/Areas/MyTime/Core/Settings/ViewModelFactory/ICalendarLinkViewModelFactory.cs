using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Settings;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.ViewModelFactory
{
	public interface ICalendarLinkViewModelFactory
	{
		CalendarLinkViewModel CreateViewModel(CalendarLinkSettings calendarLinkSettings, string actionName);
	}
}