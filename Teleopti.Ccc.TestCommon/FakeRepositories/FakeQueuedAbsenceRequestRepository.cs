using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.Util;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories 
{
	public class FakeQueuedAbsenceRequestRepository : IQueuedAbsenceRequestRepository
	{
		private readonly IList<IQueuedAbsenceRequest> _queuedRequests = new List<IQueuedAbsenceRequest>();

		public void Add(IQueuedAbsenceRequest entity)
		{
			_queuedRequests.Add(entity);
		}

		public void Remove(IQueuedAbsenceRequest entity)
		{
			_queuedRequests.Remove(entity);
		}
	
		IQueuedAbsenceRequest IRepository<IQueuedAbsenceRequest>.Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IQueuedAbsenceRequest> LoadAll()
		{
			return _queuedRequests;
		}

		public IQueuedAbsenceRequest Load(Guid id)
		{
			var reqs = _queuedRequests.Where(x => x.PersonRequest == id);
			if (reqs!=null && reqs.Any())
				return reqs.First();
			return null;
		}

		public void AddRange(IEnumerable<IQueuedAbsenceRequest> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; }

		public IQueuedAbsenceRequest Get(Guid personRequestId)
		{
			return _queuedRequests.FirstOrDefault(x => x.PersonRequest == personRequestId);
		}

		public IList<IQueuedAbsenceRequest> Find(DateTimePeriod period)
		{
			IList<IQueuedAbsenceRequest> overlappingRequests = new List<IQueuedAbsenceRequest>();
			foreach (var request in _queuedRequests)
			{
				if ((request.StartDateTime <= period.StartDateTime && request.EndDateTime >= period.StartDateTime) ||
					(request.StartDateTime <= period.EndDateTime && request.EndDateTime >= period.EndDateTime))
				{
					overlappingRequests.Add(request);
				}
			}
			return overlappingRequests;
		}

		public void Remove(IEnumerable<Guid> absenceRequests)
		{
			absenceRequests.ForEach(x =>
			{
				_queuedRequests.Remove(Load(x));
			});
		}

		public void Send(List<Guid> requestId, DateTime timeStamp)
		{
			foreach (var id in requestId)
			{
				_queuedRequests.FirstOrDefault(x => x.PersonRequest == id).Sent = timeStamp;
			}
		}
	}
}