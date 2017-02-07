using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAgentGroupRepository : IAgentGroupRepository
	{
		private readonly List<AgentGroup> _agentGroups = new List<AgentGroup>();

		public void Add(AgentGroup root)
		{
			_agentGroups.Add(root); // Should set Id
		}

		public void Remove(AgentGroup root)
		{
			_agentGroups.Remove(root);
		}

		public AgentGroup Get(Guid id)
		{
			return _agentGroups.FirstOrDefault(x => x.Id == id);
		}

		public AgentGroup Load(Guid id)
		{
			return _agentGroups.First(x => x.Id == id);
		}

		public IList<AgentGroup> LoadAll()
		{
			return _agentGroups;
		}
	}
}