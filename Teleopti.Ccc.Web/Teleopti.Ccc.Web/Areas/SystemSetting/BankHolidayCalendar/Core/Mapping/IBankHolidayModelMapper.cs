using System.Collections.Generic;
using Teleopti.Ccc.Domain.SystemSettingWeb;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Models;

namespace Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Core.Mapping
{
	public interface IBankHolidayModelMapper
	{
		IBankHolidayDate Map(IBankHolidayCalendar calendar, BankHolidayDateForm date);
		BankHolidayCalendarViewModel Map(IBankHolidayCalendar calendar);
		IEnumerable<BankHolidayCalendarViewModel> Map(IEnumerable<IBankHolidayCalendar> calendars);
		BankHolidayCalendarViewModel MapModelChanged(IBankHolidayCalendar calendar, BankHolidayCalendarForm _calendar);
	}
}