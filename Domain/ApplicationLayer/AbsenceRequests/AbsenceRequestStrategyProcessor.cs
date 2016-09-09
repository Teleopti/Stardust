using System;
using System.Collections.Generic;
using System.Linq;
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
			var allRequests = new List<IQueuedAbsenceRequest>();
			allRequests.AddRange(allRequestsRaw.Where(x => (x.EndDateTime.Subtract(x.StartDateTime)).Days <= 60 && x.Sent == null));
		
			if (allRequests.Any())
			{
				var futureRequests = getFutureRequests(allRequests, nearFuturePeriod);
				var nearFutureRequestIds = getNearFutureRequestIds(nearFutureTimeStampInterval, nearFuturePeriod, futureRequests);

				if (nearFutureRequestIds.Any())
					return nearFutureRequestIds;


				var farFutureRequests = getFarFutureRequests(allRequests, nearFuturePeriod);
				var farFutureRequestIds = getFarFutureRequestIds(farFutureTimeStampInterval, nearFuturePeriod, farFutureRequests,
																 windowSize);
				if (farFutureRequestIds.Any())
					return farFutureRequestIds;


				var pastRequests = getPastRequests(allRequests, nearFuturePeriod);
				var pastRequestIds = getPastRequestIds(nearFuturePeriod, pastRequests, windowSize);
				return pastRequestIds;

			}

			return new List<IEnumerable<Guid>>();
		}



		private IList<IQueuedAbsenceRequest> getPastRequests(IEnumerable<IQueuedAbsenceRequest> allRequests, DateTimePeriod nearFuturePeriod)
		{
			return allRequests.Where(x => x.StartDateTime.Date < nearFuturePeriod.StartDateTime.Date).ToList();	
		}

		private List<IQueuedAbsenceRequest> getFarFutureRequests(IEnumerable<IQueuedAbsenceRequest> allRequests, DateTimePeriod nearFuturePeriod)
		{
			return allRequests.Where(x => x.StartDateTime.Date > nearFuturePeriod.EndDateTime.Date).ToList();
		}

		private List<IQueuedAbsenceRequest> getFutureRequests(IEnumerable<IQueuedAbsenceRequest> allRequests, DateTimePeriod nearFuturePeriod)
		{
			return allRequests.Where(x => x.StartDateTime.Date >= nearFuturePeriod.StartDateTime.Date).ToList();
		}



		private IList<IEnumerable<Guid>> getNearFutureRequestIds(DateTime nearFutureTimeStampInterval, DateTimePeriod nearFuturePeriod, List<IQueuedAbsenceRequest> futureRequests)
		{
			var result = new List<IEnumerable<Guid>>();
			if (futureRequests.Any())
			{
				var requests = getRequestsOnPeriod(nearFutureTimeStampInterval, nearFuturePeriod, futureRequests);
				if (requests.Any())
					result.Add(requests);
			}
			return result;
		}

		private IList<IEnumerable<Guid>> getPastRequestIds(DateTimePeriod windowPeriod,
			IList<IQueuedAbsenceRequest> pastRequests, int windowSize)
		{
			var result = new List<IEnumerable<Guid>>();
			while (pastRequests.Any())
			{
				windowPeriod = new DateTimePeriod(windowPeriod.StartDateTime.AddDays(-windowSize),
					windowPeriod.StartDateTime.AddDays(-1));
				var windowRequests = getRequestsOnPeriod(windowPeriod, pastRequests);
				pastRequests = removeRequests(pastRequests, windowRequests);
				if (windowRequests.Any())
					result.Add(windowRequests);
			}
			return result;
		}

		private IList<IEnumerable<Guid>> getFarFutureRequestIds(DateTime farFutureTimeStampInterval, DateTimePeriod windowPeriod,
			IList<IQueuedAbsenceRequest> farFutureRequests, int windowSize)
		{
			var result = new List<IEnumerable<Guid>>();
			while (farFutureRequests.Any(x => x.StartDateTime >= windowPeriod.StartDateTime))
			{
				windowPeriod = new DateTimePeriod(windowPeriod.EndDateTime.AddDays(1),
					windowPeriod.EndDateTime.AddDays(windowSize));
				var windowRequests = getRequestsOnPeriod(farFutureTimeStampInterval, windowPeriod, farFutureRequests);
				farFutureRequests = removeRequests(farFutureRequests, windowRequests);
				if (windowRequests.Any())
					result.Add(windowRequests);
			}
			return result;
		}



		private IList<IQueuedAbsenceRequest> removeRequests(IList<IQueuedAbsenceRequest> removeFromList,
			List<Guid> ids)
		{
			foreach (var id in ids)
			{
				var foundRequest = removeFromList.FirstOrDefault(x => x.PersonRequest == id);
				if (foundRequest != null)
					removeFromList.Remove(foundRequest);
			}
			return removeFromList;
		}



		private List<Guid> getRequestsOnPeriod(DateTimePeriod windowPeriod,
			IList<IQueuedAbsenceRequest> requests)
		{
			var requestsInPeriod = requests.Where(y => y.StartDateTime.Date >= windowPeriod.StartDateTime.Date &&
																		 y.StartDateTime.Date <= windowPeriod.EndDateTime.Date);
			if (requestsInPeriod.Any())
			{
				var min = requestsInPeriod.Select(x => x.StartDateTime).Min();
				var max = requestsInPeriod.Select(x => x.EndDateTime).Max();

				var overlappingRequests =
					findAbsencesWithinPeriod(requests, min, max);
				if (overlappingRequests.Any())
					return overlappingRequests.Select(x => x.PersonRequest).ToList();
			}

			return new List<Guid>();
		}

		private List<Guid> getRequestsOnPeriod(DateTime interval, DateTimePeriod windowPeriod,
			IList<IQueuedAbsenceRequest> requests)
		{
			var requestsInPeriod = requests.Where(y => y.StartDateTime.Date >= windowPeriod.StartDateTime.Date &&
																		 y.StartDateTime.Date <= windowPeriod.EndDateTime.Date);
			if (requestsInPeriod.Any())
			{
				var min = requestsInPeriod.Select(x => x.StartDateTime).Min();
				var max = requestsInPeriod.Select(x => x.EndDateTime).Max();

				var overlappingRequests =
					findAbsencesWithinPeriod(requests, min, max);
				if (overlappingRequests.Any(x => x.Created <= interval))
					return overlappingRequests.Select(x => x.PersonRequest).ToList();
				removeRequests(requests,
					requests.Where(x =>   x.StartDateTime <= windowPeriod.EndDateTime).Select(x => x.PersonRequest).ToList());
			}

			return new List<Guid>();
		}



		private static IEnumerable<IQueuedAbsenceRequest> findAbsencesWithinPeriod(IList<IQueuedAbsenceRequest> requests,
			DateTime min, DateTime max)
		{
			return requests.Where(
				x =>
					(x.StartDateTime >= min && x.StartDateTime <= max) &&
					(x.EndDateTime > min && x.EndDateTime <= max));
		}
	}
}