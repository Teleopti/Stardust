using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.BankHolidayCalendar;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IBankHolidayDateRepository : IRepository<IBankHolidayDate>
	{
		IBankHolidayDate Find(DateOnly date,IBankHolidayCalendar calendar);
		IEnumerable<IBankHolidayDate> FetchByCalendarsAndPeriod(IEnumerable<IBankHolidayCalendar> calendars, DateOnlyPeriod period);
	}
}
