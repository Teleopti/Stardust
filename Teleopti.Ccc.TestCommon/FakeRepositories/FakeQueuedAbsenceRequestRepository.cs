using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
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
			if (_queuedRequests.Count == 0)
			{
				_queuedRequests.Add(entity);
				return;
			}

			if (_queuedRequests.All(queuedAbsenceRequest => queuedAbsenceRequest.PersonRequest != entity.PersonRequest))
			{
				_queuedRequests.Add(entity);
				return;
			}


			foreach (var queuedRequest in _queuedRequests)
			{
				if (queuedRequest.PersonRequest == entity.PersonRequest)
				{
					_queuedRequests.Remove(queuedRequest);
					_queuedRequests.Add(entity);
					return;
				}
			}
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

		public void CheckAndUpdateSent(int minutes)
		{
			foreach (var request in _queuedRequests)
			{
				if (request.Sent != null && request.Sent.GetValueOrDefault() < DateTime.UtcNow.AddMinutes(-minutes))
					request.Sent = null;
			}
		}

		public int UpdateRequestPeriod(Guid id, DateTimePeriod period)
		{
			
			var queuedReq = _queuedRequests.FirstOrDefault(x => x.PersonRequest == id);
			if (queuedReq.Sent.HasValue)
				return 0;
			queuedReq.StartDateTime = period.StartDateTime;
			queuedReq.EndDateTime = period.EndDateTime;
			return 1;
		}

		public IList<IQueuedAbsenceRequest> FindByPersonRequestIds(IEnumerable<Guid> personRequestIds)
		{
			return _queuedRequests.Where(q => personRequestIds.Contains(q.PersonRequest)).ToList();
		}
	}
}