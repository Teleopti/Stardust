using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class FilterRequestsWithDifferentVersion : IFilterRequestsWithDifferentVersion
	{
		private readonly IPersonRequestRepository _personRequestRepository;

		public FilterRequestsWithDifferentVersion(IPersonRequestRepository personRequestRepository)
		{
			_personRequestRepository = personRequestRepository;
		}

		public IList<IEnumerable<IQueuedAbsenceRequest>> Filter(IDictionary<Guid, int> reqVersions, IList<IEnumerable<IQueuedAbsenceRequest>> potentialBulk)
		{
			var result = new List<IEnumerable<IQueuedAbsenceRequest>>();

			var latestPersonRequests = _personRequestRepository.Find(reqVersions.Keys);
			var dirtyRequests = latestPersonRequests.Where(x =>
			{
				return
					reqVersions.Any(
						y => y.Key == x.Id.GetValueOrDefault() && y.Value != ((PersonRequest) x).Version.GetValueOrDefault());
			}).Select(x => x.Id).ToArray();
			foreach (var queuedAbsenceRequests in potentialBulk)
			{
				var bulk = queuedAbsenceRequests.Where(queuedAbsenceRequest => !dirtyRequests.Contains(queuedAbsenceRequest.PersonRequest)).ToList();
				if (bulk.Any())
					result.Add(bulk);
			}
			

			return result;

		}
	}

	public class FilterRequestsWithDifferentVersion41930ToggleOff : IFilterRequestsWithDifferentVersion
	{
		public IList<IEnumerable<IQueuedAbsenceRequest>> Filter(IDictionary<Guid, int> reqVersions, IList<IEnumerable<IQueuedAbsenceRequest>> potentialBulk)
		{
			return potentialBulk;
		}
	}

	public interface IFilterRequestsWithDifferentVersion
	{
		IList<IEnumerable<IQueuedAbsenceRequest>> Filter(IDictionary<Guid, int> reqVersions, IList<IEnumerable<IQueuedAbsenceRequest>> potentialBulk);
	}
}