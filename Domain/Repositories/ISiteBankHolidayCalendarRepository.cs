using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface ISiteBankHolidayCalendarRepository:IRepository<ISiteBankHolidayCalendar>
	{
		IEnumerable<ISiteBankHolidayCalendar> FindAllSiteBankHolidayCalendarsSortedBySite();
		ISiteBankHolidayCalendar FindSiteBankHolidayCalendar(ISite site);
	}
}
