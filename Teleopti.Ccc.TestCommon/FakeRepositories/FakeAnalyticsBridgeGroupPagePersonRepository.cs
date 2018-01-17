using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Util;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class AnalyticsBridgeGroupPagePerson
	{
		public Guid GroupPageCode { get; set; }
		public Guid BusinessUnitCode { get; set; }
		public Guid PersonCode { get; set; }
		public Guid GroupCode { get; set; }
		public int PersonId { get; set; }
	}

	public class FakeAnalyticsBridgeGroupPagePersonRepository : IAnalyticsBridgeGroupPagePersonRepository
	{
		public readonly List<AnalyticsBridgeGroupPagePerson> Bridges = new List<AnalyticsBridgeGroupPagePerson>();
		private readonly IDictionary<int, Guid> personMappings = new Dictionary<int, Guid>();

		public FakeAnalyticsBridgeGroupPagePersonRepository WithPersonMapping(Guid personCode, int personId)
		{
			personMappings[personId] = personCode;
			return this;
		}

		public FakeAnalyticsBridgeGroupPagePersonRepository Has(AnalyticsBridgeGroupPagePerson item)
		{
			Bridges.Add(item);
			return this;
		}

		public void DeleteAllBridgeGroupPagePerson(ICollection<Guid> groupPageIds, Guid businessUnitId)
		{
			Bridges.RemoveAll(x => x.BusinessUnitCode == businessUnitId && groupPageIds.Contains(x.GroupPageCode));
		}

		public void DeleteBridgeGroupPagePerson(ICollection<Guid> personIds, Guid groupId, Guid businessUnitId)
		{
			Bridges.RemoveAll(x => x.BusinessUnitCode == businessUnitId && personIds.Contains(x.PersonCode) && x.GroupCode == groupId);
		}

		public void AddBridgeGroupPagePerson(ICollection<Guid> personIds, Guid groupId, Guid businessUnitId)
		{
			personIds.ForEach(personCode =>
			{
				personMappings.Where(x => x.Value == personCode).Select(x => x.Key).ForEach(personId =>
				{
					Bridges.Add(new AnalyticsBridgeGroupPagePerson
					{
						BusinessUnitCode = businessUnitId,
						GroupCode = groupId,
						PersonCode = personCode,
						PersonId = personId
						// Should be a GroupPageCode somewhere
					});
				});
			});
		}

		public IEnumerable<Guid> GetBridgeGroupPagePerson(Guid groupId, Guid businessUnitId)
		{
			return Bridges.Where(x => x.GroupCode == groupId && x.BusinessUnitCode == businessUnitId).Select(x => x.PersonCode).ToList();
		}

		public IEnumerable<Guid> GetGroupPagesForPersonPeriod(int personId, Guid businessUnitId)
		{
			return Bridges.Where(x => x.PersonCode == personMappings[personId] && x.BusinessUnitCode == businessUnitId).Select(x => x.GroupCode).ToList();
		}

		public void DeleteBridgeGroupPagePersonForPersonPeriod(int personId, ICollection<Guid> groupIds, Guid businessUnitId)
		{
			Bridges.RemoveAll(
				x => x.PersonCode == personMappings[personId] && x.BusinessUnitCode == businessUnitId && groupIds.Contains(x.GroupCode));
		}

		public void AddBridgeGroupPagePersonForPersonPeriod(int personId, ICollection<Guid> groupIds, Guid businessUnitId)
		{
			groupIds.ForEach(groupId => Bridges.Add(new AnalyticsBridgeGroupPagePerson
			{
				BusinessUnitCode = businessUnitId,
				GroupCode = groupId,
				PersonCode = personMappings[personId],
				PersonId = personId
				//Should probably have a GroupPageCode somewhere
			}));
		}

		public void DeleteBridgeGroupPagePersonExcludingPersonPeriods(Guid personCode, ICollection<int> personPeriodIds)
		{
			Bridges.RemoveAll(
				x => x.PersonCode == personCode && !personPeriodIds.Contains(x.PersonId));
		}

		public void DeleteAllForPersons(Guid groupPageId, ICollection<Guid> personIds, Guid businessUnitId)
		{
			Bridges.RemoveAll(x => x.BusinessUnitCode == businessUnitId && x.GroupPageCode == groupPageId && personIds.Contains(x.PersonCode));
		}
	}
}