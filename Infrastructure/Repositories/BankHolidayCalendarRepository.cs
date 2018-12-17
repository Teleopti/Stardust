using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public interface IBankHolidayCalendarRepository: IRepository<IBankHolidayCalendar>
	{
	}

	public class BankHolidayCalendarRepository: Repository<IBankHolidayCalendar>, IBankHolidayCalendarRepository
	{
		public BankHolidayCalendarRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}
	}
}
