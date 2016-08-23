using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public interface IAbsenceRequestStrategyProcessor
	{
		IEnumerable<Guid> Get(DateTime interval, DateTime farFutureInterval, DateTimePeriod nearFuture);
	}
	public class AbsenceRequestStrategyProcessor : IAbsenceRequestStrategyProcessor
	{
		private readonly IQueuedAbsenceRequestRepository _queuedAbsenceRequestRepo;

		public AbsenceRequestStrategyProcessor(IQueuedAbsenceRequestRepository queuedAbsenceRequestRepo)
		{
			_queuedAbsenceRequestRepo = queuedAbsenceRequestRepo;
		}

		public IEnumerable<Guid> Get(DateTime nearFutureInterval,DateTime farFutureInterval,  DateTimePeriod nearFuture)
		{
			var allRequests = _queuedAbsenceRequestRepo.LoadAll();
			var nearFutureReuqests = getNearFuture(nearFutureInterval, nearFuture, allRequests);
			if (nearFutureReuqests.Any())
				return nearFutureReuqests;
			return getFarFutureRequests(farFutureInterval, nearFuture, allRequests);

		}

		private IEnumerable<Guid> getFarFutureRequests(DateTime farFutureInterval, DateTimePeriod nearFuture, IList<IQueuedAbsenceRequest> allRequests)
		{
			var farFutureList = allRequests.Where(y => y.StartDateTime >= nearFuture.EndDateTime);
			if (farFutureList.Any(x => x.Created <= farFutureInterval))
				return farFutureList.Select(x => x.PersonRequest).ToList();
			return new List<Guid>();
		}

		private List<Guid> getNearFuture(DateTime nearFutureInterval, DateTimePeriod nearFuture, IList<IQueuedAbsenceRequest> allRequests)
		{
			var nearFutureList = allRequests.Where(y => y.StartDateTime >= nearFuture.StartDateTime &&
																	  y.StartDateTime <= nearFuture.EndDateTime);
			if (nearFutureList.Any(x => x.Created <= nearFutureInterval))
				return nearFutureList.Select(x => x.PersonRequest).ToList();
			return new List<Guid>();
		}
	}
}