using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.BankHolidayCalendar;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class SiteBankHolidayCalendarRepository : Repository<ISiteBankHolidayCalendar>, ISiteBankHolidayCalendarRepository
	{
		public SiteBankHolidayCalendarRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
		{
		}

		public IEnumerable<ISiteBankHolidayCalendar> FindAllSiteBankHolidayCalendarsSortedBySite()
		{
			ICollection<ISiteBankHolidayCalendar> retList = Session.CreateCriteria(typeof(ISiteBankHolidayCalendar))
				.AddOrder(Order.Asc("Site"))
				.List<ISiteBankHolidayCalendar>();
			return retList;
		}

		public ISiteBankHolidayCalendar FindSiteBankHolidayCalendar(ISite site)
		{
			if (site == null) return null;

			var ret = Session.CreateCriteria<SiteBankHolidayCalendar>()
				.Add(Restrictions.Eq("Site", site))
				.UniqueResult<ISiteBankHolidayCalendar>();
			return ret;
		}
	}
}
