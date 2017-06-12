using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class PlanningGroupRepository :  Repository<IPlanningGroup>, IPlanningGroupRepository
	{
		public PlanningGroupRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}
	}
}