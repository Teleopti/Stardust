using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Wfm.Adherence.Configuration;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeRtaStateGroupRepositoryLegacy : FakeRtaStateGroupRepository
	{
		public FakeRtaStateGroupRepositoryLegacy() : base(null)
		{
		}
	}

	public class FakeRtaStateGroupRepository : IRtaStateGroupRepository
	{
		private readonly IFakeStorage _storage;

		public FakeRtaStateGroupRepository(IFakeStorage storage)
		{
			_storage = storage ?? new FakeStorageSimple();
		}

		public void Has(IRtaStateGroup stateGroup)
		{
			Add(stateGroup);
		}

		public void Add(IRtaStateGroup entity)
		{
			_storage.Add(entity);
		}

		public void Remove(IRtaStateGroup entity)
		{
			throw new NotImplementedException();
		}

		public IRtaStateGroup Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IRtaStateGroup> LoadAll()
		{
			return _storage.LoadAll<IRtaStateGroup>().ToList();
		}

		public IRtaStateGroup Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IRtaStateGroup> LoadAllCompleteGraph()
		{
			return _storage.LoadAll<IRtaStateGroup>().ToList();
		}
	}
}