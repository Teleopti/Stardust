using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Ccc.Domain.SystemSetting.BankHolidayCalendar;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class BankHolidayDateRepository : Repository<IBankHolidayDate>, IBankHolidayDateRepository
	{
		public BankHolidayDateRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}
	}
}
