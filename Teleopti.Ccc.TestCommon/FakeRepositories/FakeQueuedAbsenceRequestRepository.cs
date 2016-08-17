using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories 
{
	public class FakeQueuedAbsenceRequestRepository : IQueuedAbsenceRequestRepository
	{
		private readonly IList<QueuedAbsenceRequest> _queuedAbsenceRequestRepository = new List<QueuedAbsenceRequest>();

		public void Add(QueuedAbsenceRequest entity)
		{
			_queuedAbsenceRequestRepository.Add(entity);
		}

		public void Remove(QueuedAbsenceRequest entity)
		{
			_queuedAbsenceRequestRepository.Remove(entity);
		}

		public void Add(IQueuedAbsenceRequest root)
		{
			throw new NotImplementedException();
		}

		public void Remove(IQueuedAbsenceRequest root)
		{
			throw new NotImplementedException();
		}

		IQueuedAbsenceRequest IRepository<IQueuedAbsenceRequest>.Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IQueuedAbsenceRequest> LoadAll()
		{
			throw new NotImplementedException();
		}

		public IQueuedAbsenceRequest Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IQueuedAbsenceRequest> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; }

		public QueuedAbsenceRequest Get(Guid personRequestId)
		{
			return _queuedAbsenceRequestRepository.FirstOrDefault(x => x.PersonRequestId == personRequestId);
		}

		public IList<QueuedAbsenceRequest> Find(DateTimePeriod period)
		{
			IList<QueuedAbsenceRequest> overlappingRequests = new List<QueuedAbsenceRequest>();
			foreach (var request in _queuedAbsenceRequestRepository)
			{
				if ((request.StarDateTime < period.StartDateTime && request.EndDateTime > period.StartDateTime) ||
					(request.StarDateTime < period.EndDateTime && request.EndDateTime > period.EndDateTime))
				{
					overlappingRequests.Add(request);
				}
			}
			return overlappingRequests;
		}
	}
}