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

		public IEnumerable<Guid> Get(DateTime nearFutureInterval, DateTime farFutureInterval, DateTimePeriod nearFuture)
		{
			var allRequests = _queuedAbsenceRequestRepo.LoadAll();
			if (allRequests.Any())
			{
				var nearFutureReuqests = getNearFuture(nearFutureInterval, nearFuture, allRequests);
				if (nearFutureReuqests.Any())
					return nearFutureReuqests;
				return getFarFutureRequests(farFutureInterval, nearFuture, allRequests);
			}

			return new List<Guid>();
		}

		private IEnumerable<Guid> getFarFutureRequests(DateTime farFutureInterval, DateTimePeriod nearFuture,
			IList<IQueuedAbsenceRequest> allRequests)
		{
			var farFutureList = allRequests.Where(y => y.StartDateTime >= nearFuture.EndDateTime);
			if (farFutureList.Any(x => x.Created <= farFutureInterval))
				return farFutureList.Select(x => x.PersonRequest).ToList();
			return new List<Guid>();
		}

		private List<Guid> getNearFuture(DateTime nearFutureInterval, DateTimePeriod nearFuture,
			IList<IQueuedAbsenceRequest> allRequests)
		{
			var nearFutureList = allRequests.Where(y => y.StartDateTime >= nearFuture.StartDateTime &&
																	  y.StartDateTime <= nearFuture.EndDateTime);
			if (nearFutureList.Any())
			{
				var min = nearFutureList.Select(x => x.StartDateTime).Min();
				var max = nearFutureList.Select(x => x.EndDateTime).Max();

				var overlappingRequests =
					findAbsencesWithinPeriod(allRequests, min, max);
				if (overlappingRequests.Any(x => x.Created <= nearFutureInterval))
					return overlappingRequests.Select(x => x.PersonRequest).ToList();
			}
			return new List<Guid>();
		}

		private static IEnumerable<IQueuedAbsenceRequest> findAbsencesWithinPeriod(IList<IQueuedAbsenceRequest> allRequests,
			DateTime min, DateTime max)
		{
			return allRequests.Where(
				x =>
					(x.StartDateTime >= min && x.StartDateTime <= max) &&
					(x.EndDateTime > min && x.EndDateTime <= max));
		}
	}
}