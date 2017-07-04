using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class QueuedOvertimeRequestRepository : Repository<IQueuedOvertimeRequest>, IQueuedOvertimeRequestRepository
	{
		public QueuedOvertimeRequestRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}
	}
}
