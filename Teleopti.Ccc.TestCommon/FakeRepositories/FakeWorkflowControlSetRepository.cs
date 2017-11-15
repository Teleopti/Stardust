using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeWorkflowControlSetRepository : IWorkflowControlSetRepository
	{
		private readonly FakeStorage _storage;

		public FakeWorkflowControlSetRepository(FakeStorage storage)
		{
			_storage = storage;
		}

		public void Add(IWorkflowControlSet root)
		{
			_storage.Add(root);
		}

		public void Remove(IWorkflowControlSet root)
		{
			throw new NotImplementedException();
		}

		public IWorkflowControlSet Get(Guid id)
		{
			return _storage.Get<IWorkflowControlSet>(id);
		}

		public IList<IWorkflowControlSet> LoadAll()
		{
			return _storage.LoadAll<IWorkflowControlSet>().ToArray();
		}

		public IWorkflowControlSet Load(Guid id)
		{
			return Get(id);
		}

		public IList<IWorkflowControlSet> LoadAllSortByName()
		{
			throw new NotImplementedException();
		}
	}
}