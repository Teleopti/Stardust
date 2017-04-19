using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;

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
			((IDeleteTag)Get(root.Id.GetValueOrDefault())).SetDeleted();
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

		public FakeAgentGroupRepository Has(IAgentGroup root)
		{
			_agentGroups.Add(root);
			return this;
		}
	}
}