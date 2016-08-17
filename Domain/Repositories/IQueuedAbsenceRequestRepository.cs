using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IQueuedAbsenceRequestRepository
	{
		void Add(QueuedAbsenceRequest entity);
		void Remove(Guid personRequestId);
		QueuedAbsenceRequest Get(Guid personRequestId);
		IList<QueuedAbsenceRequest> Find(Guid businessUnit, DateTimePeriod period);
	}
}