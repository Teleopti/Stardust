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

		public void Remove(Guid personRequestId)
		{
			throw new NotImplementedException();
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
		public IList<QueuedAbsenceRequest> Find(DateTimePeriod period)
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