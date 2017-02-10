using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class AgentGroupRepository :  Repository<IAgentGroup>, IAgentGroupRepository
	{
		public AgentGroupRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}
	}
}