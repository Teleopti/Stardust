using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.BankHolidayCalendar;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface ISiteBankHolidayCalendarRepository:IRepository<ISiteBankHolidayCalendar>
	{
		IEnumerable<ISiteBankHolidayCalendar> FindAllSiteBankHolidayCalendarsSortedBySite();
		ISiteBankHolidayCalendar FindSiteBankHolidayCalendar(ISite site);
		IEnumerable<SiteBankHolidayCalendar> FindSiteBankHolidayCalendars(Guid calendarId);
	}
}
