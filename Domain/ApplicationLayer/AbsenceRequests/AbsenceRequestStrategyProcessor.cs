using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public interface IAbsenceRequestStrategyProcessor
	{
		IList<IEnumerable<Guid>> Get(DateTime interval, DateTime farFutureTimeStampInterval, DateTimePeriod nearFuturePeriod,
			int windowSize);
	}

	public class AbsenceRequestStrategyProcessor : IAbsenceRequestStrategyProcessor
	{
		private readonly IQueuedAbsenceRequestRepository _queuedAbsenceRequestRepo;

		public AbsenceRequestStrategyProcessor(IQueuedAbsenceRequestRepository queuedAbsenceRequestRepo)
		{
			_queuedAbsenceRequestRepo = queuedAbsenceRequestRepo;
		}

		public IList<IEnumerable<Guid>> Get(DateTime nearFutureTimeStampInterval, DateTime farFutureTimeStampInterval,
											DateTimePeriod nearFuturePeriod,
											int windowSize)
		{
			var allRequestsRaw = _queuedAbsenceRequestRepo.LoadAll();
			var allNewRequests = new List<IQueuedAbsenceRequest>();
			allNewRequests.AddRange(allRequestsRaw.Where(x => (x.EndDateTime.Subtract(x.StartDateTime)).Days <= 60 && x.Sent == null));
			var processingPeriods = getProcessingPeriods(allRequestsRaw.Where(x => x.Sent != null));
			var notConflictingRequests = getNotConflictingRequests(allNewRequests, processingPeriods);

			if (!notConflictingRequests.Any()) return new List<IEnumerable<Guid>>();
			var futureRequests = getFutureRequests(notConflictingRequests, nearFuturePeriod);
			var nearFutureRequestIds = getNearFutureRequestIds(nearFutureTimeStampInterval, nearFuturePeriod, futureRequests);

			if (nearFutureRequestIds.Any())
				return nearFutureRequestIds;

			var farFutureRequests = getFarFutureRequests(notConflictingRequests, nearFuturePeriod);
			var farFutureRequestIds = getFarFutureRequestIds(farFutureTimeStampInterval, nearFuturePeriod, farFutureRequests,
				windowSize);
			if (farFutureRequestIds.Any())
				return farFutureRequestIds;

			var pastRequests = getPastRequests(notConflictingRequests, nearFuturePeriod);
			var pastRequestIds = getPastRequestIds(nearFuturePeriod, pastRequests, windowSize);
			return pastRequestIds;
		}

		private static List<IQueuedAbsenceRequest> getNotConflictingRequests(IEnumerable<IQueuedAbsenceRequest> allNewRequests, IReadOnlyCollection<DateTimePeriod> processingPeriods)
		{
			return allNewRequests.Where(request => !isInPeriod(processingPeriods, request)).ToList();
		}

		private static bool isInPeriod(IEnumerable<DateTimePeriod> processingPeriods, IQueuedAbsenceRequest request)
		{
			return processingPeriods.Any(period => period.ContainsPart(new DateTimePeriod(request.StartDateTime.Utc(), request.EndDateTime.Utc())));
		}

		private static List<DateTimePeriod> getProcessingPeriods(IEnumerable<IQueuedAbsenceRequest> processingRequests)
		{
			var groupedRequests = new ConcurrentDictionary<DateTime, List<IQueuedAbsenceRequest>>();
			foreach (var request in processingRequests)
			{
				groupedRequests.AddOrUpdate(request.Sent.GetValueOrDefault(), new List<IQueuedAbsenceRequest>() {request}, (key, oldValue) =>
				{
					oldValue.Add(request);
					return oldValue;
				});
			}

			var periods = new List<DateTimePeriod>();
			foreach (var timeStamp in groupedRequests.Keys)
			{
				var min = DateTime.MaxValue;
				var max = DateTime.MinValue;
				
				var requestsWithSameTimeStamp = groupedRequests[timeStamp];
				foreach (var request in requestsWithSameTimeStamp)
				{
					if (request.StartDateTime < min)
						min = request.StartDateTime;
					if (request.EndDateTime > max)
						max = request.EndDateTime;
				}
				if(min < max)
					periods.Add(new DateTimePeriod(min.Utc(), max.Utc()));
			}
			return periods;
		}


		private static IList<IQueuedAbsenceRequest> getPastRequests(IEnumerable<IQueuedAbsenceRequest> allRequests, DateTimePeriod nearFuturePeriod)
		{
			return allRequests.Where(x => x.StartDateTime.Date < nearFuturePeriod.StartDateTime.Date).ToList();	
		}

		private static List<IQueuedAbsenceRequest> getFarFutureRequests(IEnumerable<IQueuedAbsenceRequest> allRequests, DateTimePeriod nearFuturePeriod)
		{
			return allRequests.Where(x => x.StartDateTime.Date > nearFuturePeriod.EndDateTime.Date).ToList();
		}

		private static List<IQueuedAbsenceRequest> getFutureRequests(IEnumerable<IQueuedAbsenceRequest> allRequests, DateTimePeriod nearFuturePeriod)
		{
			return allRequests.Where(x => x.StartDateTime.Date >= nearFuturePeriod.StartDateTime.Date).ToList();
		}

		private static IList<IEnumerable<Guid>> getNearFutureRequestIds(DateTime nearFutureTimeStampInterval, DateTimePeriod nearFuturePeriod, IList<IQueuedAbsenceRequest> futureRequests)
		{
			var result = new List<IEnumerable<Guid>>();
			if (!futureRequests.Any()) return result;
			var requests = getRequestsOnPeriod(nearFutureTimeStampInterval, nearFuturePeriod, futureRequests);
			if (requests.Any())
				result.Add(requests);
			return result;
		}

		private static IList<IEnumerable<Guid>> getPastRequestIds(DateTimePeriod windowPeriod,
			IList<IQueuedAbsenceRequest> pastRequests, int windowSize)
		{
			var result = new List<IEnumerable<Guid>>();
			while (pastRequests.Any())
			{
				windowPeriod = new DateTimePeriod(windowPeriod.StartDateTime.AddDays(-windowSize).Utc(),
					windowPeriod.StartDateTime.AddDays(-1).Utc());
				var windowRequests = getRequestsOnPeriod(windowPeriod, pastRequests);
				pastRequests = removeRequests(pastRequests, windowRequests);
				if (windowRequests.Any())
					result.Add(windowRequests);
			}
			return result;
		}

		private static IList<IEnumerable<Guid>> getFarFutureRequestIds(DateTime farFutureTimeStampInterval, DateTimePeriod windowPeriod,
			IList<IQueuedAbsenceRequest> farFutureRequests, int windowSize)
		{
			var result = new List<IEnumerable<Guid>>();
			while (farFutureRequests.Any(x => x.StartDateTime >= windowPeriod.StartDateTime))
			{
				windowPeriod = new DateTimePeriod(windowPeriod.EndDateTime.AddDays(1).Utc(),
					windowPeriod.EndDateTime.AddDays(windowSize).Utc());
				var windowRequests = getRequestsOnPeriod(farFutureTimeStampInterval, windowPeriod, farFutureRequests);
				farFutureRequests = removeRequests(farFutureRequests, windowRequests);
				if (windowRequests.Any())
					result.Add(windowRequests);
			}
			return result;
		}

		private static IList<IQueuedAbsenceRequest> removeRequests(IList<IQueuedAbsenceRequest> removeFromList,
			IEnumerable<Guid> ids)
		{
			foreach (var id in ids)
			{
				var foundRequest = removeFromList.FirstOrDefault(x => x.PersonRequest == id);
				if (foundRequest != null)
					removeFromList.Remove(foundRequest);
			}
			return removeFromList;
		}

		private static List<Guid> getRequestsOnPeriod(DateTimePeriod windowPeriod,
			IList<IQueuedAbsenceRequest> requests)
		{
			var requestsInPeriod = requests.Where(y => y.StartDateTime.Date >= windowPeriod.StartDateTime.Date &&
																		 y.StartDateTime.Date <= windowPeriod.EndDateTime.Date).ToList();
			if (!requestsInPeriod.Any()) return new List<Guid>();
			var min = requestsInPeriod.Select(x => x.StartDateTime).Min();
			var max = requestsInPeriod.Select(x => x.EndDateTime).Max();

			var overlappingRequests =
				findAbsencesWithinPeriod(requests, min, max).ToList();
			return overlappingRequests.Any() ? overlappingRequests.Select(x => x.PersonRequest).ToList() : new List<Guid>();
		}

		private static List<Guid> getRequestsOnPeriod(DateTime interval, DateTimePeriod windowPeriod,
			IList<IQueuedAbsenceRequest> requests)
		{
			var requestsInPeriod = requests.Where(y => y.StartDateTime.Date >= windowPeriod.StartDateTime.Date &&
																		 y.StartDateTime.Date <= windowPeriod.EndDateTime.Date).ToList();
			if (!requestsInPeriod.Any()) return new List<Guid>();
			var min = requestsInPeriod.Select(x => x.StartDateTime).Min();
			var max = requestsInPeriod.Select(x => x.EndDateTime).Max();

			var overlappingRequests =
				findAbsencesWithinPeriod(requests, min, max).ToList();
			if (overlappingRequests.Any(x => x.Created <= interval))
				return overlappingRequests.Select(x => x.PersonRequest).ToList();
			removeRequests(requests,
				requests.Where(x =>   x.StartDateTime <= windowPeriod.EndDateTime).Select(x => x.PersonRequest).ToList());

			return new List<Guid>();
		}

		private static IEnumerable<IQueuedAbsenceRequest> findAbsencesWithinPeriod(IEnumerable<IQueuedAbsenceRequest> requests,
			DateTime min, DateTime max)
		{
			return requests.Where(
				x =>
					(x.StartDateTime >= min && x.StartDateTime <= max) &&
					(x.EndDateTime > min && x.EndDateTime <= max));
		}
	}
}