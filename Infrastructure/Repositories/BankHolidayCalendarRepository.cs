using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Ccc.Domain.SystemSetting.BankHolidayCalendar;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class BankHolidayCalendarRepository : Repository<IBankHolidayCalendar>, IBankHolidayCalendarRepository
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		
		public BankHolidayCalendarRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}
	}
}
