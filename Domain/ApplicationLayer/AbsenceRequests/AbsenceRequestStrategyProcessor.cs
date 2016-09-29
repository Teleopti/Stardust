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
		IList<IEnumerable<Guid>> Get(DateTime nearFutureThresholdTime, DateTime farFutureThresholdTime, DateTime pastThresholdTime, DateOnlyPeriod initialPeriod,
									 int windowSize);
	}

	public class AbsenceRequestStrategyProcessor : IAbsenceRequestStrategyProcessor
	{
		private readonly IQueuedAbsenceRequestRepository _queuedAbsenceRequestRepo;

		public AbsenceRequestStrategyProcessor(IQueuedAbsenceRequestRepository queuedAbsenceRequestRepo)
		{
			_queuedAbsenceRequestRepo = queuedAbsenceRequestRepo;
		}

		public IList<IEnumerable<Guid>> Get(DateTime nearFutureThresholdTime, DateTime farFutureThresholdTime, DateTime pastThresholdTime,
											DateOnlyPeriod initialPeriod, int windowSize)
		{
			var allRequestsRaw = _queuedAbsenceRequestRepo.LoadAll();
			var allNewRequests = new List<IQueuedAbsenceRequest>();
			allNewRequests.AddRange(allRequestsRaw.Where(x => (x.EndDateTime.Subtract(x.StartDateTime)).Days <= 60 && x.Sent == null));

			//filter out requests that is not in a period that is already beeing processed
			var processingPeriods = getProcessingPeriods(allRequestsRaw.Where(x => x.Sent != null));
			var notConflictingRequests = getNotConflictingRequests(allNewRequests, processingPeriods);
			if (!notConflictingRequests.Any()) return new List<IEnumerable<Guid>>();

			var futureRequests = getFutureRequests(notConflictingRequests, initialPeriod);
			var nearFutureRequestIds = getNearFutureRequestIds(nearFutureThresholdTime, initialPeriod, futureRequests);

			if (nearFutureRequestIds.Any())
				return nearFutureRequestIds;

			var farFutureRequests = getFarFutureRequests(notConflictingRequests, initialPeriod);
			var farFutureRequestIds = getFarFutureRequestIds(farFutureThresholdTime, initialPeriod, farFutureRequests, windowSize);
			if (farFutureRequestIds.Any())
				return farFutureRequestIds;

			var pastRequests = getPastRequests(notConflictingRequests, initialPeriod);
			var pastRequestIds = getPastRequestIds(pastThresholdTime, initialPeriod, pastRequests, windowSize);
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
				if (min < max)
					periods.Add(new DateTimePeriod(min.Utc(), max.Utc()));
			}
			return periods;
		}


		private static IList<IQueuedAbsenceRequest> getPastRequests(IEnumerable<IQueuedAbsenceRequest> allRequests, DateOnlyPeriod initialPeriod)
		{
			return allRequests.Where(x => x.StartDateTime.Date < initialPeriod.StartDate.Date).ToList();
		}

		private static List<IQueuedAbsenceRequest> getFarFutureRequests(IEnumerable<IQueuedAbsenceRequest> allRequests, DateOnlyPeriod initialPeriod)
		{
			return allRequests.Where(x => x.StartDateTime.Date > initialPeriod.EndDate.Date).ToList();
		}

		private static List<IQueuedAbsenceRequest> getFutureRequests(IEnumerable<IQueuedAbsenceRequest> allRequests, DateOnlyPeriod nearFuturePeriod)
		{
			return allRequests.Where(x => x.StartDateTime.Date >= nearFuturePeriod.StartDate.Date).ToList();
		}

		private static IList<IEnumerable<Guid>> getNearFutureRequestIds(DateTime nearFutureThresholdTime, DateOnlyPeriod nearFuturePeriod, IList<IQueuedAbsenceRequest> futureRequests)
		{
			var result = new List<IEnumerable<Guid>>();
			if (!futureRequests.Any()) return result;

			var requests = getRequestsOnPeriod(nearFutureThresholdTime, nearFuturePeriod, futureRequests);
			if (requests.Any())
				result.Add(requests);

			return result;
		}

		private static IList<IEnumerable<Guid>> getPastRequestIds(DateTime pastThresholdTime, DateOnlyPeriod period, IList<IQueuedAbsenceRequest> pastRequests, int windowSize)
		{
			var result = new List<IEnumerable<Guid>>();
			//Add Min check to stop in time if a request is "missed in the list" bug #40891
			while (pastRequests.Any(x => x.StartDateTime.Date <= period.StartDate.Date) && period.StartDate.Date > DateTime.MinValue.AddYears(1)) 
			{
				period = new DateOnlyPeriod(period.StartDate.AddDays(-windowSize),
												  period.StartDate.AddDays(-1));

				var windowRequests = getRequestsOnPeriod(pastThresholdTime, period, pastRequests);
				if (windowRequests.Any())
					result.Add(windowRequests);
			}
			return result;
		}

		private static IList<IEnumerable<Guid>> getFarFutureRequestIds(DateTime farFutureThresholdTime, DateOnlyPeriod period,
																	   IList<IQueuedAbsenceRequest> farFutureRequests, int windowSize)
		{
			var result = new List<IEnumerable<Guid>>();
			//Add Min check to stop in time if a request is "missed in the list" bug #40891
			while (farFutureRequests.Any(x => x.StartDateTime.Date >= period.StartDate.Date) && period.StartDate.Date < DateTime.MaxValue.AddYears(-1))
			{
				period = new DateOnlyPeriod(period.EndDate.AddDays(1),
											period.EndDate.AddDays(windowSize));
				var windowRequests = getRequestsOnPeriod(farFutureThresholdTime, period, farFutureRequests);
				if (windowRequests.Any())
					result.Add(windowRequests);
			}
			return result;
		}

		private static void removeRequests(IList<IQueuedAbsenceRequest> removeFromList, IEnumerable<Guid> ids)
		{
			foreach (var id in ids)
			{
				var foundRequest = removeFromList.FirstOrDefault(x => x.PersonRequest == id);
				if (foundRequest != null)
					removeFromList.Remove(foundRequest);
			}
		}

		private static List<Guid> getRequestsOnPeriod(DateTime thresholdTime, DateOnlyPeriod windowPeriod,
													  IList<IQueuedAbsenceRequest> requests)
		{
			var requestsInPeriod = requests.Where(x => x.StartDateTime.Date >= windowPeriod.StartDate.Date &&
													   x.StartDateTime.Date <= windowPeriod.EndDate.Date).ToList();
			if (!requestsInPeriod.Any()) return new List<Guid>();

			var min = requestsInPeriod.Select(x => x.StartDateTime).Min();
			var max = requestsInPeriod.Select(x => x.EndDateTime).Max();

			var requestsinExtendedPeriod = findRequestsCompletelyWithinPeriod(requests, min, max).ToList();

			if (requestsinExtendedPeriod.Any(x => x.Created <= thresholdTime))
			{
				removeRequests(requests, requestsinExtendedPeriod.Select(x => x.PersonRequest));
				return requestsinExtendedPeriod.Select(x => x.PersonRequest).ToList();
			}
			removeRequests(requests, requestsInPeriod.Select(x => x.PersonRequest));

			return new List<Guid>();
		}

		private static IEnumerable<IQueuedAbsenceRequest> findRequestsCompletelyWithinPeriod(IEnumerable<IQueuedAbsenceRequest> requests,
																							 DateTime min, DateTime max)
		{
			return requests.Where(
				x => (x.StartDateTime >= min && x.StartDateTime <= max) &&
					 (x.EndDateTime > min && x.EndDateTime <= max));
		}
	}
}