using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class SeniorityWorkDayRanksRepository : Repository<ISeniorityWorkDayRanks>
	{
		public SeniorityWorkDayRanksRepository(IUnitOfWork unitOfWork): base(unitOfWork)
		{
		}
	}
}
