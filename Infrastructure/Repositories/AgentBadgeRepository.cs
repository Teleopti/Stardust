using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class AgentBadgeRepository : Repository<AgentBadge>
	{
		public AgentBadgeRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
		{
		}

		public AgentBadgeRepository(IUnitOfWorkFactory unitOfWorkFactory) : base(unitOfWorkFactory)
		{
		}

		public AgentBadgeRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}
	}
}