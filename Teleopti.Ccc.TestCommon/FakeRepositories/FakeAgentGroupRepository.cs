using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAgentGroupRepository : IAgentGroupRepository
	{
		private readonly List<IAgentGroup> _agentGroups = new List<IAgentGroup>();

		public void Add(IAgentGroup root)
		{
			_agentGroups.Add(root); // Should set Id
		}

		public void Remove(IAgentGroup root)
		{
			_agentGroups.Remove(root);
		}

		public IAgentGroup Get(Guid id)
		{
			return _agentGroups.FirstOrDefault(x => x.Id == id);
		}

		public IAgentGroup Load(Guid id)
		{
			return _agentGroups.First(x => x.Id == id);
		}

		public IList<IAgentGroup> LoadAll()
		{
			return _agentGroups;
		}
	}
}