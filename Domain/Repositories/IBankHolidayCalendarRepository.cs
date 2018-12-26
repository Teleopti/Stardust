using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSettingWeb;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IBankHolidayCalendarRepository : IRepository<IBankHolidayCalendar>
	{
		void Delete(Guid Id);
	}
}
