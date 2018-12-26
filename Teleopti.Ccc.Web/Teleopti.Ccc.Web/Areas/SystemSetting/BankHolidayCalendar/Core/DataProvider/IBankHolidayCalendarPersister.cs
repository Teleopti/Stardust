using System;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Models;

namespace Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Core.DataProvider
{
	public interface IBankHolidayCalendarPersister
	{
		BankHolidayCalendarViewModel Persist(BankHolidayCalendarForm input);
		bool Delete(Guid Id);
	}
}