using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
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

		public IList<IEnumerable<Guid>> Filter(IDictionary<Guid, int> reqVersions, IList<IEnumerable<Guid>> potentialBulk)
		{
			var result = new List<IEnumerable<Guid>>();
			
				var latestPersonRequests = _personRequestRepository.Find(reqVersions.Keys);
				var dirtyRequests = latestPersonRequests.Where(x =>
				{
					return
						reqVersions.Any(
							y => y.Key == x.Id.GetValueOrDefault() && y.Value != ((PersonRequest)x).Version.GetValueOrDefault());
				});
				foreach (var guids in potentialBulk)
				{
					var bulk = (List<Guid>)guids;
					foreach (var dirtyReq in dirtyRequests)
					{
						if (bulk.Contains(dirtyReq.Id.GetValueOrDefault()))
							bulk.Remove(dirtyReq.Id.GetValueOrDefault());
					}
					if(bulk.Any())
						result.Add(bulk);
				}

				return result;
			
		}
	}

	public class FilterRequestsWithDifferentVersion41930ToggleOff : IFilterRequestsWithDifferentVersion
	{
		public IList<IEnumerable<Guid>> Filter(IDictionary<Guid, int> reqVersions, IList<IEnumerable<Guid>> potentialBulk)
		{
			return potentialBulk;
		}
	}

	public interface IFilterRequestsWithDifferentVersion
	{
		IList<IEnumerable<Guid>> Filter(IDictionary<Guid, int> reqVersions, IList<IEnumerable<Guid>> potentialBulk);
	}
}