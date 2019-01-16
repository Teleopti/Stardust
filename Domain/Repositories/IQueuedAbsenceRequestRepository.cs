using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IQueuedAbsenceRequestRepository: IRepository<IQueuedAbsenceRequest>
	{
		IList<IQueuedAbsenceRequest> Find( DateTimePeriod period);
		void Remove(DateTime sent);
		void Send(List<Guid> queuedId, DateTime timeStamp);
		int UpdateRequestPeriod(Guid id, DateTimePeriod period);
		IList<IQueuedAbsenceRequest> FindByPersonRequestIds(IEnumerable<Guid> personRequestIds);
		void ResetSent(DateTime eventSent);
		void ResetSent(List<Guid> ids);
		void Remove(List<Guid> ids);
		IList<IQueuedAbsenceRequest> FindByIds(IList<Guid> ids);
	}
}