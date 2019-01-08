using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Models;

namespace Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Core.DataProvider
{
	public interface ISiteBankHolidayCalendarsProvider
	{
		IEnumerable<SiteBankHolidayCalendarsViewModel> GetAllSettings();
	}
}