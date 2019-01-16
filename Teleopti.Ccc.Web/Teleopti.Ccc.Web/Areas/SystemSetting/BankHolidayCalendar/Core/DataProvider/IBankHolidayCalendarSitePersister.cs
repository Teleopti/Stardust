using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Models;

namespace Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Core.DataProvider
{
	public interface IBankHolidayCalendarSitePersister
	{
		bool UpdateCalendarsForSites(IEnumerable<SiteBankHolidayCalendarsViewModel> input);
	}
}