using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeRtaStateGroupRepositoryLegacy : FakeRtaStateGroupRepository
	{
		public FakeRtaStateGroupRepositoryLegacy() : base(new FakeStorage())
		{
		}
	}

	public class FakeRtaStateGroupRepository : IRtaStateGroupRepository
	{
		private readonly FakeStorage _storage;

		public FakeRtaStateGroupRepository(FakeStorage storage)
		{
			_storage = storage;
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

		public IList<IRtaStateGroup> LoadAll()
		{
			return _storage.LoadAll<IRtaStateGroup>().ToList();
		}

		public IRtaStateGroup Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IRtaStateGroup> LoadAllCompleteGraph()
		{
			return _storage.LoadAll<IRtaStateGroup>().ToList();
		}
	}
}