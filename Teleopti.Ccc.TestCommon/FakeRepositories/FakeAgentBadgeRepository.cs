﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAgentBadgeRepository: IAgentBadgeRepository
	{
		private IList<AgentBadge> _agentBadges = new List<AgentBadge>();
		private int _findByPersonListWasCalledTimes = 0;

		public void Add(AgentBadge badge)
		{
			_agentBadges.Add(badge);
		}

		public void Add(IList<AgentBadge> agentBadges)
		{
			foreach (var agentBadge in agentBadges)
			{
				Add(agentBadge);
			}
		}

		public void Remove(AgentBadge agentBadge)
		{
			_agentBadges.Remove(agentBadge);
		}

		public void ClearAll()
		{
			_agentBadges.Clear();
		}

		public ICollection<AgentBadge> Find(IEnumerable<Guid> personIdList, BadgeType badgeType)
		{
			return (from agentBadge in _agentBadges
				from personId in personIdList
				where personId == agentBadge.Person 
					  && badgeType == agentBadge.BadgeType
				select agentBadge).ToList();
		}

		public ICollection<AgentBadge> Find(IEnumerable<Guid> personIdList)
		{
			_findByPersonListWasCalledTimes++;
			return (from agentBadge in _agentBadges
				from personId in personIdList
				where personId == agentBadge.Person
				select agentBadge).ToList();
		}

		public AgentBadge Find(IPerson person, BadgeType badgeType)
		{
			return _agentBadges.FirstOrDefault(x => x.Person == person.Id.Value && x.BadgeType == badgeType);
		}

		public int FindByPersonListCalledTimes()
		{
			return _findByPersonListWasCalledTimes;
		}
	}
}
