using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeWorkflowControlSetRepository : IWorkflowControlSetRepository
	{
		private IList<IWorkflowControlSet> _workflowControlSets = new List<IWorkflowControlSet>();

		public void Add(IWorkflowControlSet root)
		{
			throw new NotImplementedException();
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

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IWorkflowControlSet> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		public IList<IWorkflowControlSet> LoadAllSortByName()
		{
			throw new NotImplementedException();
		}
	}
}