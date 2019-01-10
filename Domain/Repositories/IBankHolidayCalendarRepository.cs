using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.BankHolidayCalendar;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IBankHolidayCalendarRepository : IRepository<IBankHolidayCalendar>
	{
		ICollection<IBankHolidayCalendar> FindBankHolidayCalendars(IEnumerable<Guid> ids);
	}
}
