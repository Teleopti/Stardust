using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public interface IAbsenceRequestStrategyProcessor
	{
		IList<IEnumerable<Guid>> Get(DateTime interval, DateTime farFutureTimeStampInterval, DateTimePeriod nearFuturePeriod, int windowSize);
	}

	public class AbsenceRequestStrategyProcessor : IAbsenceRequestStrategyProcessor
	{
		private readonly IQueuedAbsenceRequestRepository _queuedAbsenceRequestRepo;

		public AbsenceRequestStrategyProcessor(IQueuedAbsenceRequestRepository queuedAbsenceRequestRepo)
		{
			_queuedAbsenceRequestRepo = queuedAbsenceRequestRepo;
		}

		public IList<IEnumerable<Guid>> Get(DateTime nearFutureTimeStampInterval, DateTime farFutureTimeStampInterval, DateTimePeriod nearFuturePeriod,
			int windowSize)
		{
			var allRequests = _queuedAbsenceRequestRepo.LoadAll();
			var tempAllRequests = new List<IQueuedAbsenceRequest>();
			tempAllRequests.AddRange(allRequests.Where(x=>(x.EndDateTime.Subtract(x.StartDateTime)).Days <=60 ));
			if (allRequests.Any())
			{
				var nearFutureReuqests = getRequestsOnPeriod(nearFutureTimeStampInterval, nearFuturePeriod, tempAllRequests);
				if (nearFutureReuqests.Any())
					return new List<IEnumerable<Guid>> {nearFutureReuqests};
				var farFutureRequests = getFarFutureRequests(farFutureTimeStampInterval, nearFuturePeriod, tempAllRequests, windowSize);
				if (farFutureRequests.Any())
					return farFutureRequests;
				return getPastRequests(nearFuturePeriod, tempAllRequests, windowSize);
			}

			return new List<IEnumerable<Guid>>();
		}

		private IList<IEnumerable<Guid>> getPastRequests(DateTimePeriod windowPeriod, IList<IQueuedAbsenceRequest> tempAllRequests, int windowSize)
		{
			var result = new List<IEnumerable<Guid>>();
			while (tempAllRequests.Any())
			{
				windowPeriod = new DateTimePeriod(windowPeriod.StartDateTime.AddDays(-windowSize),
					windowPeriod.StartDateTime.AddDays(-1));
				var windowRequests = getRequestsOnPeriod(windowPeriod, tempAllRequests);
				tempAllRequests = removeRequests(tempAllRequests, windowRequests);
				if (windowRequests.Any())
					result.Add(windowRequests);
			}
			return result;
		}
		
		private IList<IEnumerable<Guid>> getFarFutureRequests(DateTime farFutureTimeStampInterval, DateTimePeriod windowPeriod,
			IList<IQueuedAbsenceRequest> tempRequests, int windowSize)
		{
			var result = new List<IEnumerable<Guid>>();
			while (tempRequests.Any(x => x.StartDateTime >= windowPeriod.StartDateTime))
			{
				windowPeriod = new DateTimePeriod(windowPeriod.EndDateTime.AddDays(1),
					windowPeriod.EndDateTime.AddDays(windowSize));
				var windowRequests = getRequestsOnPeriod(farFutureTimeStampInterval, windowPeriod, tempRequests);
				tempRequests = removeRequests(tempRequests, windowRequests);
				if (windowRequests.Any())
					result.Add(windowRequests);
			}
			return result;
		}

		private IList<IQueuedAbsenceRequest> removeRequests(IList<IQueuedAbsenceRequest> removeFromList,
			List<Guid> requestsToBeRemoved)
		{
			foreach (var id in requestsToBeRemoved)
			{
				var foundRequest = removeFromList.FirstOrDefault(x => x.PersonRequest == id);
				if (foundRequest != null)
					removeFromList.Remove(foundRequest);
			}
			return removeFromList;
		}

		private List<Guid> getRequestsOnPeriod(DateTimePeriod windowPeriod,
											   IList<IQueuedAbsenceRequest> allRequests)
		{
			var requestsInPeriod = allRequests.Where(y => y.StartDateTime.Date >= windowPeriod.StartDateTime.Date &&
																		 y.StartDateTime.Date <= windowPeriod.EndDateTime.Date);
			if (requestsInPeriod.Any())
			{
				var min = requestsInPeriod.Select(x => x.StartDateTime).Min();
				var max = requestsInPeriod.Select(x => x.EndDateTime).Max();

				var overlappingRequests =
					findAbsencesWithinPeriod(allRequests, min, max);
				if (overlappingRequests.Any())
					return overlappingRequests.Select(x => x.PersonRequest).ToList();
				}

			return new List<Guid>();
		}

		private List<Guid> getRequestsOnPeriod(DateTime interval, DateTimePeriod windowPeriod,
			IList<IQueuedAbsenceRequest> allRequests)
		{
			var requestsInPeriod = allRequests.Where(y => y.StartDateTime.Date >= windowPeriod.StartDateTime.Date &&
																		 y.StartDateTime.Date <= windowPeriod.EndDateTime.Date);
			if (requestsInPeriod.Any())
			{
				var min = requestsInPeriod.Select(x => x.StartDateTime).Min();
				var max = requestsInPeriod.Select(x => x.EndDateTime).Max();

				var overlappingRequests =
					findAbsencesWithinPeriod(allRequests, min, max);
				if (overlappingRequests.Any(x => x.Created <= interval))
					return overlappingRequests.Select(x => x.PersonRequest).ToList();
				removeRequests(allRequests,
					allRequests.Where(x => x.StartDateTime <= windowPeriod.EndDateTime).Select(x => x.PersonRequest).ToList());
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