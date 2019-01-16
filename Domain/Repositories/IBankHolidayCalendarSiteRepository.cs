using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.BankHolidayCalendar;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IBankHolidayCalendarSiteRepository : IRepository<IBankHolidayCalendarSite>
	{
		IEnumerable<Guid> FindSitesByCalendar(Guid calendarId);
	}
}
