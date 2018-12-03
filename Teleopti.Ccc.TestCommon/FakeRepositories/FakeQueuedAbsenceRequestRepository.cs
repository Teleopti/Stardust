using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;


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

		public IEnumerable<IQueuedAbsenceRequest> LoadAll()
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

		public IUnitOfWork UnitOfWork { get; }
		public bool UpdateRequestPeriodWasCalled { get; set; }

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

		public void Remove(DateTime sent)
		{
			var sentRequests = _queuedRequests.Where(y => y.Sent == sent).ToList();
			foreach (var request in sentRequests)
			{
				_queuedRequests.Remove(request);
			}
		}

		public void Send(List<Guid> queuedId, DateTime timeStamp)
		{
			foreach (var id in queuedId)
			{
				_queuedRequests.FirstOrDefault(x => x.Id == id).Sent = timeStamp;
			}
		}

		public int UpdateRequestPeriod(Guid id, DateTimePeriod period)
		{
			
			var queuedReq = _queuedRequests.FirstOrDefault(x => x.PersonRequest == id);
			if (queuedReq == null)
				return 0;
			if (queuedReq.Sent.HasValue)
				return 0;
			queuedReq.StartDateTime = period.StartDateTime;
			queuedReq.EndDateTime = period.EndDateTime;
			UpdateRequestPeriodWasCalled = true;
			return 1;
		}

		public IList<IQueuedAbsenceRequest> FindByPersonRequestIds(IEnumerable<Guid> personRequestIds)
		{
			return _queuedRequests.Where(q => personRequestIds.Contains(q.PersonRequest)).ToList();
		}

		public void ResetSent(DateTime eventSent)
		{
			var sentRequests = _queuedRequests.Where(y => y.Sent == eventSent).ToList();
			foreach (var request in sentRequests)
			{
				request.Sent = null;
			}
		}
	}
}