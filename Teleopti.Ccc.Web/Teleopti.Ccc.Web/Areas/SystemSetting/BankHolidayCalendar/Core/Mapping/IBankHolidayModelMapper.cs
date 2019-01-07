using System.Collections.Generic;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Models;

namespace Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Core.Mapping
{
	public interface IBankHolidayModelMapper
	{
		BankHolidayCalendarViewModel Map(IBankHolidayCalendar calendar, IEnumerable<IBankHolidayDate> dates);
		IEnumerable<BankHolidayCalendarViewModel> Map(IEnumerable<IBankHolidayCalendar> calendars, IEnumerable<IBankHolidayDate> dates);
	}
}