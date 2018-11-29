using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public interface IAbsenceRequestStrategyProcessor
	{
		IList<IEnumerable<IQueuedAbsenceRequest>> Get(DateTime threshholdTime, DateTime pastThresholdTime, DateOnlyPeriod initialPeriod,
									 int windowSize);
	}

	public class AbsenceRequestStrategyProcessor : IAbsenceRequestStrategyProcessor
	{
		private readonly IQueuedAbsenceRequestRepository _queuedAbsenceRequestRepo;
		private readonly DenyLongQueuedAbsenceRequests _denyLongQueuedAbsenceRequests;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly INow _now;

		public AbsenceRequestStrategyProcessor(IQueuedAbsenceRequestRepository queuedAbsenceRequestRepo,
			DenyLongQueuedAbsenceRequests denyLongQueuedAbsenceRequests,
			ICurrentUnitOfWorkFactory currentUnitOfWorkFactory,
			INow now)
		{
			_queuedAbsenceRequestRepo = queuedAbsenceRequestRepo;
			_denyLongQueuedAbsenceRequests = denyLongQueuedAbsenceRequests;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_now = now;
		}

		public IList<IEnumerable<IQueuedAbsenceRequest>> Get(DateTime threshholdTime, DateTime pastThresholdTime, DateOnlyPeriod initialPeriod, int windowSize)
		{

			IList<IQueuedAbsenceRequest> requestsInQueue;
			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var allRequestsRaw = _queuedAbsenceRequestRepo.LoadAll();
				requestsInQueue = _denyLongQueuedAbsenceRequests.DenyAndRemoveLongRunningRequests(allRequestsRaw);
				uow.PersistAll();
			}

			var timedOutRequestsTime = _now.UtcDateTime().AddMinutes(-60);
			var allNewRequests = new List<IQueuedAbsenceRequest>();
			allNewRequests.AddRange(requestsInQueue.Where(x => x.Sent == null || x.Sent < timedOutRequestsTime));

			//filter out requests that is not in a period that is already beeing processed
			var processingPeriods = getProcessingPeriods(requestsInQueue.Where(x => x.Sent != null && x.Sent >= timedOutRequestsTime));

			var notConflictingRequests = getNotConflictingRequests(allNewRequests, processingPeriods);
			if (!notConflictingRequests.Any()) return new List<IEnumerable<IQueuedAbsenceRequest>>();

			var futureRequests = getFutureRequests(notConflictingRequests, initialPeriod);
			var nearFutureRequestIds = getNearFutureRequestIds(threshholdTime, initialPeriod, futureRequests, windowSize);

			if (nearFutureRequestIds.Any())
			{
				return nearFutureRequestIds;
			}

			var farFutureRequests = getFarFutureRequests(notConflictingRequests, initialPeriod);
			var farFutureRequestIds = getFarFutureRequestIds(threshholdTime, initialPeriod, farFutureRequests, windowSize);
			if (farFutureRequestIds.Any())
			{
				return farFutureRequestIds;
			}

			var pastRequests = getPastRequests(notConflictingRequests, initialPeriod);
			var pastRequestIds = getPastRequestIds(initialPeriod, pastRequests, windowSize);
			return pastRequestIds;
		}

		private static List<IQueuedAbsenceRequest> getNotConflictingRequests(IEnumerable<IQueuedAbsenceRequest> allNewRequests, IReadOnlyCollection<DateTimePeriod> processingPeriods)
		{
			return allNewRequests.Where(request => !isInPeriod(processingPeriods, request)).ToList();
		}

		private static bool isInPeriod(IEnumerable<DateTimePeriod> processingPeriods, IQueuedAbsenceRequest request)
		{
			var requestPeriod = new DateTimePeriod(request.StartDateTime.Utc(), request.EndDateTime.Utc());
			return processingPeriods.Any(period => period.ContainsPart(requestPeriod));
		}

		private static List<DateTimePeriod> getProcessingPeriods(IEnumerable<IQueuedAbsenceRequest> processingRequests)
		{
			var groupedRequests = processingRequests.GroupBy(p => p.Sent.GetValueOrDefault());

			var periods = new List<DateTimePeriod>();
			foreach (var timeStamp in groupedRequests)
			{
				var min = DateTime.MaxValue;
				var max = DateTime.MinValue;

				foreach (var request in timeStamp)
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

		private IList<IEnumerable<IQueuedAbsenceRequest>> getNearFutureRequestIds(DateTime nearFutureThresholdTime, DateOnlyPeriod nearFuturePeriod, IList<IQueuedAbsenceRequest> futureRequests, int windowSize)
		{
			var result = new List<IEnumerable<IQueuedAbsenceRequest>>();
			if (!futureRequests.Any()) return result;
			var requestsInPeriod = futureRequests.Where(x => x.StartDateTime.Date >= nearFuturePeriod.StartDate.Date &&
													   x.StartDateTime.Date <= nearFuturePeriod.EndDate.Date).ToArray().Where(x => x.Created <= nearFutureThresholdTime);
			if (!requestsInPeriod.Any()) return result;

			var requests = getRequestsOnPeriod(nearFuturePeriod, futureRequests, windowSize);
			if (requests.Any())
				result.Add(requests);

			return result;
		}

		private IList<IEnumerable<IQueuedAbsenceRequest>> getPastRequestIds(DateOnlyPeriod period, IList<IQueuedAbsenceRequest> pastRequests, int windowSize)
		{
			var result = new List<IEnumerable<IQueuedAbsenceRequest>>();
			//Add Min check to stop in time if a request is "missed in the list" bug #40891
			while (pastRequests.Any(x => x.StartDateTime.Date <= period.StartDate.Date) && period.StartDate.Date > DateTime.MinValue.AddYears(1))
			{
				period = new DateOnlyPeriod(period.StartDate.AddDays(-windowSize),
												  period.StartDate.AddDays(-1));

				var windowRequests = getRequestsOnPeriod(period, pastRequests, windowSize);
				if (windowRequests.Any())
					result.Add(windowRequests);
			}
			return result;
		}

		private IList<IEnumerable<IQueuedAbsenceRequest>> getFarFutureRequestIds(DateTime farFutureThresholdTime, DateOnlyPeriod period,
																	   IList<IQueuedAbsenceRequest> farFutureRequests, int windowSize)
		{
			var result = new List<IEnumerable<IQueuedAbsenceRequest>>();
			var requestsInPeriod = farFutureRequests.Where(x => x.StartDateTime.Date >= period.StartDate.Date).Where(x => x.Created <= farFutureThresholdTime);
			if (!requestsInPeriod.Any()) return result;

			//Add Min check to stop in time if a request is "missed in the list" bug #40891
			while (farFutureRequests.Any(x => x.StartDateTime.Date >= period.StartDate.Date) && period.StartDate.Date < DateTime.MaxValue.AddYears(-1))
			{
				period = new DateOnlyPeriod(period.EndDate.AddDays(1), period.EndDate.AddDays(windowSize));
				var windowRequests = getRequestsOnPeriod(period, farFutureRequests, windowSize);
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

		private List<IQueuedAbsenceRequest> getRequestsOnPeriod(DateOnlyPeriod windowPeriod,
													  IList<IQueuedAbsenceRequest> requests, int windowSize)
		{
			var requestsInPeriod = requests.Where(x => x.StartDateTime.Date >= windowPeriod.StartDate.Date &&
													   x.StartDateTime.Date <= windowPeriod.EndDate.Date).ToArray();
			if (!requestsInPeriod.Any()) return new List<IQueuedAbsenceRequest>();

			var min = requestsInPeriod.Select(x => x.StartDateTime).Min();
			var maxreqEndDate = requestsInPeriod.Select(x => x.EndDateTime).Max();
			var max = maxreqEndDate;
			var nextWindowEndDate = windowPeriod.EndDate.AddDays(windowSize + 1);
			if (maxreqEndDate > nextWindowEndDate.Date.AddMinutes(-1))
				max = nextWindowEndDate.Date.AddMinutes(-1);


			var requestsinExtendedPeriod = findRequestsCompletelyWithinPeriod(requests, min, max).ToList();

			if (requestsinExtendedPeriod.Any())
			{
				removeRequests(requests, requestsinExtendedPeriod.Select(x => x.PersonRequest));
				return requestsinExtendedPeriod.ToList();
			}
			removeRequests(requests, requestsInPeriod.Select(x => x.PersonRequest));

			return new List<IQueuedAbsenceRequest>();
		}

		private static IEnumerable<IQueuedAbsenceRequest> findRequestsCompletelyWithinPeriod(IEnumerable<IQueuedAbsenceRequest> requests,
																							 DateTime min, DateTime max)
		{
			if (min > max) throw new Exception();
			return requests.Where(
				x => x.StartDateTime >= min && x.StartDateTime <= max);
		}
	}
}