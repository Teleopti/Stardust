using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAgentBadgeWithRankRepository: IAgentBadgeWithRankRepository
	{
		private IList<IAgentBadgeWithRank> _agentBadgeWithRanks = new List<IAgentBadgeWithRank>();
		public void Add(IAgentBadgeWithRank agentBadgeWithRank)
		{
			_agentBadgeWithRanks.Add(agentBadgeWithRank);
		}

		public void Add(IList<IAgentBadgeWithRank> agentBadgeWithRanks)
		{
			foreach (var agentBadgeWithRank in agentBadgeWithRanks)
			{
				Add(agentBadgeWithRank);
			}
		}

		public void ClearAll()
		{
			_agentBadgeWithRanks.Clear();
		}

		public void Remove(IAgentBadgeWithRank agentBadgeWithRank)
		{
			_agentBadgeWithRanks.Remove(agentBadgeWithRank);
		}

		public IAgentBadgeWithRank Get(Guid id)
		{
			return _agentBadgeWithRanks.FirstOrDefault(x => x.Id == id);
		}

		public IAgentBadgeWithRank Load(Guid id)
		{
			return Get(id);
		}

		public IEnumerable<IAgentBadgeWithRank> LoadAll()
		{
			return _agentBadgeWithRanks;
		}

		public ICollection<IAgentBadgeWithRank> Find(IEnumerable<Guid> personIdList, int badgeType)
		{
			return (from agentBadgeWithRank in _agentBadgeWithRanks
				from personId in personIdList
				where agentBadgeWithRank.Person == personId
					  && agentBadgeWithRank.BadgeType == badgeType
				select agentBadgeWithRank).ToList();
		}

		public ICollection<IAgentBadgeWithRank> Find(IEnumerable<Guid> personIdList)
		{
			return (from agentBadgeWithRank in _agentBadgeWithRanks
					from personId in personIdList
					where agentBadgeWithRank.Person == personId
					select agentBadgeWithRank).ToList();
		}

		public ICollection<IAgentBadgeWithRank> Find(IPerson person)
		{
			return _agentBadgeWithRanks.Where(agentBadgeWithRank => agentBadgeWithRank.Person == person.Id.Value).ToList();
		}

		public IAgentBadgeWithRank Find(IPerson person, int badgeType, bool isExternal)
		{
			return _agentBadgeWithRanks.FirstOrDefault(x => x.Person == person.Id.Value 
			                                                && x.BadgeType == badgeType 
			                                                && x.IsExternal == isExternal);
		}

		public IAgentBadgeWithRank Find(IPerson person, int badgeType, bool isExternal, DateOnlyPeriod period)
		{
			return _agentBadgeWithRanks.FirstOrDefault(x => x.Person == person.Id.Value
															&& x.BadgeType == badgeType
															&& x.IsExternal == isExternal
															&& period.Contains(new DateOnly(x.LastCalculatedDate)));
		}
	}
}
