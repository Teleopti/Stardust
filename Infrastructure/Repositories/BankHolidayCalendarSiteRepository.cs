using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.BankHolidayCalendar;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class BankHolidayCalendarSiteRepository : Repository<IBankHolidayCalendarSite>, IBankHolidayCalendarSiteRepository
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public BankHolidayCalendarSiteRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

		public IEnumerable<Guid> FindSitesByCalendar(Guid calendarId)
		{
			const string sql = @"select [Site] from [dbo].[BankHolidayCalendarSite] where Calendar=:calendarId";
			var query = _currentUnitOfWork.Current().Session().CreateSQLQuery(sql)
				.SetGuid("calendarId", calendarId)
				.SetReadOnly(true)
				.List<Guid>();

			return query;
		}

	}
}
