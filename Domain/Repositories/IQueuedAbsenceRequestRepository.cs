using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IQueuedAbsenceRequestRepository: IRepository<IQueuedAbsenceRequest>
	{
		IList<QueuedAbsenceRequest> Find( DateTimePeriod period);
	}
}