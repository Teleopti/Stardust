using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class QueuedAbsenceRequestRepository : Repository<IQueuedAbsenceRequest>, IQueuedAbsenceRequestRepository
	{
		public QueuedAbsenceRequestRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
		{

		}

		public IList<IQueuedAbsenceRequest> Find(DateTimePeriod period)
		{
			throw new System.NotImplementedException();
		}
	}
}