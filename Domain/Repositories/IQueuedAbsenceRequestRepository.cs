using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IQueuedAbsenceRequestRepository: IRepository<IQueuedAbsenceRequest>
	{
		IList<IQueuedAbsenceRequest> Find( DateTimePeriod period);
		void Remove(DateTime sent);
		void Send(List<Guid> requestIds, DateTime timeStamp);
		void CheckAndUpdateSent(int minutes);
	}
}