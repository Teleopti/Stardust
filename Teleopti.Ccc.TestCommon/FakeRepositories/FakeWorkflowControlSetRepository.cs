﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeWorkflowControlSetRepository : IWorkflowControlSetRepository
	{
		private readonly IFakeStorage _storage;

		public FakeWorkflowControlSetRepository(IFakeStorage storage)
		{
			_storage = storage;
		}

		public void Add(IWorkflowControlSet root)
		{
			_storage.Add(root);
		}

		public void Remove(IWorkflowControlSet root)
		{
			_storage.Remove(root);
		}

		public IWorkflowControlSet Get(Guid id)
		{
			return _storage.Get<IWorkflowControlSet>(id);
		}

		public IEnumerable<IWorkflowControlSet> LoadAll()
		{
			return _storage.LoadAll<IWorkflowControlSet>().ToArray();
		}

		public IWorkflowControlSet Load(Guid id)
		{
			return Get(id);
		}

		public IList<IWorkflowControlSet> LoadAllSortByName()
		{
			return LoadAll().OrderBy(w => w.Name).ToList();
		}
	}
}