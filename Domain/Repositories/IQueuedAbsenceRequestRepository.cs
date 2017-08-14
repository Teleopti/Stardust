using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IQueuedAbsenceRequestRepository: IRepository<IQueuedAbsenceRequest>
	{
		IList<IQueuedAbsenceRequest> Find( DateTimePeriod period);
		void Remove(DateTime sent);
		void Send(List<Guid> queuedId, DateTime timeStamp);
		void CheckAndUpdateSent(int minutes);
		int UpdateRequestPeriod(Guid id, DateTimePeriod period);
		IList<IQueuedAbsenceRequest> FindByPersonRequestIds(IEnumerable<Guid> personRequestIds);
		void ResetSent(DateTime eventSent);
	}
}