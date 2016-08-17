using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories 
{
	public class FakeQueuedAbsenceRequestRepository : IQueuedAbsenceRequestRepository
	{
		private readonly IList<QueuedAbsenceRequest> _queuedAbsenceRequestRepository = new List<QueuedAbsenceRequest>();

		public void Add(QueuedAbsenceRequest entity)
		{
			_queuedAbsenceRequestRepository.Add(entity);
		}

		public void Remove(Guid personRequestId)
		{
			throw new NotImplementedException();
		}

		public QueuedAbsenceRequest Get(Guid personRequestId)
		{
			return _queuedAbsenceRequestRepository.FirstOrDefault(x => x.PersonRequestId == personRequestId);
		}

		public IList<QueuedAbsenceRequest> Find(Guid businessUnit, DateTimePeriod period)
		{
			throw new NotImplementedException();
		}
	}
}