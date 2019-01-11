using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
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

		public IEnumerable<SiteBankHolidayCalendarMatchResult> FindSiteBankHolidayCalendar(IBankHolidayCalendar calendar)
		{
			if (calendar == null)
				return null;

			const string sql = @"select a.BankHolidayCalendar,b.Site,b.Id from [dbo].[BankHolidayCalendarsForSite] as a 
								inner join [dbo].[SiteBankHolidayCalendar] as b
								on a.Parent=b.Id 
								where a.BankHolidayCalendar=:BankHolidayCalendar";
			var query = Session.CreateSQLQuery(sql)
				.SetGuid("BankHolidayCalendar", calendar.Id.Value)
				.SetReadOnly(true)
				.List<object[]>()
				.Select(s => new SiteBankHolidayCalendarMatchResult
				{
					CalendarId = (Guid)s[0],
					SiteId = (Guid)s[1],
					SiteBankHolidayCalendarId = (Guid)s[2]
				});

			return query;
		}
	}
}
