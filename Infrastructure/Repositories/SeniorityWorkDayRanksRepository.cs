using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

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
