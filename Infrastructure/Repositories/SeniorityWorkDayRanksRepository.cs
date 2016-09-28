using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class SeniorityWorkDayRanksRepository : Repository<ISeniorityWorkDayRanks>, ISeniorityWorkDayRanksRepository
	{
#pragma warning disable 618
		public SeniorityWorkDayRanksRepository(IUnitOfWork unitOfWork): base(unitOfWork)
#pragma warning restore 618
		{
		}
	}
}
