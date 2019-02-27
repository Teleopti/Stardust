using System;
using NHibernate.Criterion;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.BankHolidayCalendar;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class BankHolidayDateRepository : Repository<IBankHolidayDate>, IBankHolidayDateRepository
	{
		public static BankHolidayDateRepository DONT_USE_CTOR(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new BankHolidayDateRepository(currentUnitOfWork, null, null);
		}

		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public BankHolidayDateRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy) 
			: base(currentUnitOfWork, currentBusinessUnit, updatedBy)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

		public IBankHolidayDate Find(DateOnly date, IBankHolidayCalendar calendar)
		{
			if (calendar == null)
				return null;

			_currentUnitOfWork.Current().Session().DisableFilter("deletedFlagFilter");

			var result = _currentUnitOfWork.Current().Session()
				.QueryOver<IBankHolidayDate>()
				.Where(d => d.Date == date && d.Calendar == calendar)
				.SingleOrDefault<IBankHolidayDate>();

			_currentUnitOfWork.Current().Session().EnableFilter("deletedFlagFilter");

			return result;
		}

		public IEnumerable<IBankHolidayDate> FetchByCalendarsAndPeriod(IEnumerable<IBankHolidayCalendar> calendars, DateOnlyPeriod period)
		{
			if (calendars == null || !calendars.Any())
				return Enumerable.Empty<IBankHolidayDate>();

			var result = _currentUnitOfWork.Current().Session()
				.QueryOver<IBankHolidayDate>()
				.Where(Restrictions.In("Calendar", calendars.ToArray()))
				.Where(d => d.Date >= period.StartDate && d.Date <= period.EndDate)
				.List<IBankHolidayDate>();

			return result;
		}
	}
}
