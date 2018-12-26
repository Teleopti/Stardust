using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSettingWeb;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class BankHolidayCalendarRepository : Repository<IBankHolidayCalendar>, IBankHolidayCalendarRepository
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		
		public BankHolidayCalendarRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

		public void Delete(Guid Id)
		{
			_currentUnitOfWork.Current().Session().CreateSQLQuery(
					$@"update [dbo].[BankHolidayCalendar] set IsDeleted = 1, UpdatedOn=:datetime where Id=:Id
					   update [dbo].[BankHolidayDate] set IsDeleted = 1, UpdatedOn=:datetime where CalendarId=:Id")
				.SetDateTime("datetime", DateTime.UtcNow)
				.SetGuid("Id", Id)
				.ExecuteUpdate();
		}
	}
}
