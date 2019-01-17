using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.BankHolidayCalendar;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class BankHolidayDateRepository : Repository<IBankHolidayDate>, IBankHolidayDateRepository
	{

		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public BankHolidayDateRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

		public IBankHolidayDate Find(DateOnly date, IBankHolidayCalendar calendar)
		{
			if (calendar == null)
				return null;

			const string sql = @"select [Id], [Date] ,[Description], [IsDeleted] from [dbo].[BankHolidayDate] where Date=:Date and Calendar=:Calendar";
			var query = _currentUnitOfWork.Current().Session().CreateSQLQuery(sql)
				.SetDateOnly("Date", date)
				.SetGuid("Calendar", calendar.Id.Value)
				.SetReadOnly(true)
				.UniqueResult<object[]>();

			if (query == null)
				return null;

			var result = new BankHolidayDate
			{
				Calendar = calendar,
				Date = new DateOnly(Convert.ToDateTime(query[1])),
				Description = query[2].ToString()
			};

			result.SetId((Guid)query[0]);
			if (Convert.ToBoolean(query[3])) result.SetDeleted();

			return result;
		}
	}
}
