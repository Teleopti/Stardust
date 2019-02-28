using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.BankHolidayCalendar;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class BankHolidayCalendarRepository : Repository<IBankHolidayCalendar>, IBankHolidayCalendarRepository
	{
		public static BankHolidayCalendarRepository DONT_USE_CTOR(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new BankHolidayCalendarRepository(currentUnitOfWork, null, null);
		}

		public BankHolidayCalendarRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy) 
			: base(currentUnitOfWork, currentBusinessUnit, updatedBy)
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
