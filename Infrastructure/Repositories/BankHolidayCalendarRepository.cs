using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.BankHolidayCalendar;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class BankHolidayCalendarRepository : Repository<IBankHolidayCalendar>, IBankHolidayCalendarRepository
	{
		public BankHolidayCalendarRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork, null, null)
		{
		}

		public ICollection<IBankHolidayCalendar> FindBankHolidayCalendars(IEnumerable<Guid> ids)
		{
			if (ids == null || !ids.Any()) return new List<IBankHolidayCalendar>();

			var ret = Session.CreateCriteria<BankHolidayCalendar>()
				.Add(Restrictions.InG("Id", ids))
				.List<IBankHolidayCalendar>();
			return ret;
		}
	}
}
