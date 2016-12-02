using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class FilterRequestsWithDifferentVersion : IFilterRequestsWithDifferentVersion
	{
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		public FilterRequestsWithDifferentVersion(IPersonRequestRepository personRequestRepository, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_personRequestRepository = personRequestRepository;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

		public IList<IEnumerable<Guid>> Filter(IDictionary<Guid, int> reqVersions, IList<IEnumerable<Guid>> potentialBulk)
		{
			var result = new List<IEnumerable<Guid>>();
			using (_currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
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
	}

	public class NoFilterRequestsWithDifferentVersion : IFilterRequestsWithDifferentVersion
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