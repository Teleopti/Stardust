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
		private readonly IList<IQueuedAbsenceRequest> _queuedAbsenceRequestRepository = new List<IQueuedAbsenceRequest>();

		public void Add(IQueuedAbsenceRequest entity)
		{
			_queuedAbsenceRequestRepository.Add(entity);
		}

		public void Remove(IQueuedAbsenceRequest entity)
		{
			_queuedAbsenceRequestRepository.Remove(entity);
		}
	
		IQueuedAbsenceRequest IRepository<IQueuedAbsenceRequest>.Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IQueuedAbsenceRequest> LoadAll()
		{
			return _queuedAbsenceRequestRepository;
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

		public IQueuedAbsenceRequest Get(Guid personRequestId)
		{
			return _queuedAbsenceRequestRepository.FirstOrDefault(x => x.PersonRequest.Id.Value == personRequestId);
		}

		public IList<IQueuedAbsenceRequest> Find(DateTimePeriod period)
		{
			IList<IQueuedAbsenceRequest> overlappingRequests = new List<IQueuedAbsenceRequest>();
			foreach (var request in _queuedAbsenceRequestRepository)
			{
				if ((request.StartDateTime < period.StartDateTime && request.EndDateTime > period.StartDateTime) ||
					(request.StartDateTime < period.EndDateTime && request.EndDateTime > period.EndDateTime))
				{
					overlappingRequests.Add(request);
				}
			}
			return overlappingRequests;
		}
	}
}