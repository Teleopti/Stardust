using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IQueuedAbsenceRequestRepository: IRepository<IQueuedAbsenceRequest>
	{
		IList<IQueuedAbsenceRequest> Find( DateTimePeriod period);
		void Remove(IEnumerable<Guid> absenceRequests);
		void Send(List<Guid> requestId, DateTime timeStamp);
		void CheckAndUpdateSent(int minutes);
	}
}