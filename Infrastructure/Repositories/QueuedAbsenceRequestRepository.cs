using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class QueuedAbsenceRequestRepository : Repository<IQueuedAbsenceRequest>, IQueuedAbsenceRequestRepository
	{
		public QueuedAbsenceRequestRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
		public void Remove(QueuedAbsenceRequest root)
		{
			throw new NotImplementedException();
		}

		{

		{
			throw new NotImplementedException();
		}

		public IList<QueuedAbsenceRequest> LoadAll()
		{
			throw new NotImplementedException();
		}

		public QueuedAbsenceRequest Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<QueuedAbsenceRequest> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; }
		public IList<QueuedAbsenceRequest> Find(DateTimePeriod period)
		}

		public IList<QueuedAbsenceRequest> Find(DateTimePeriod period)
		{
			throw new System.NotImplementedException();
		}
	}
}