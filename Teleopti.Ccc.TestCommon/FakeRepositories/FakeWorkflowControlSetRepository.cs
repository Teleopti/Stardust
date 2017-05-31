using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeWorkflowControlSetRepository : IWorkflowControlSetRepository
	{
		private readonly IList<IWorkflowControlSet> _workflowControlSets = new List<IWorkflowControlSet>();

		public void Add(IWorkflowControlSet root)
		{
			_workflowControlSets.Add(root);
		}

		public void Remove(IWorkflowControlSet root)
		{
			throw new NotImplementedException();
		}

		public IWorkflowControlSet Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IWorkflowControlSet> LoadAll()
		{
			return _workflowControlSets.ToArray();
		}

		public IWorkflowControlSet Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IWorkflowControlSet> LoadAllSortByName()
		{
			throw new NotImplementedException();
		}
	}
}