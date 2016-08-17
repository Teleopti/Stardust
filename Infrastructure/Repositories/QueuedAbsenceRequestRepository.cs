using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	class QueuedAbsenceRequestRepository : IQueuedAbsenceRequestRepository
	{
		public void Add(QueuedAbsenceRequest entity)
		{
			throw new NotImplementedException();
		}

		public void Remove(Guid personRequestId)
		{
			throw new NotImplementedException();
		}

		public QueuedAbsenceRequest Get(Guid personRequestId)
		{
			throw new NotImplementedException();
		}

		public IList<QueuedAbsenceRequest> Find(Guid businessUnit, DateTimePeriod period)
		{
			throw new NotImplementedException();
		}
	}
}