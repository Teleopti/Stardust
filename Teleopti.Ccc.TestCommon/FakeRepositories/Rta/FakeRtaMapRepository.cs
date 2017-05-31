using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeRtaMapRepository : IRtaMapRepository
	{
		private readonly FakeStorage _storage;

		public FakeRtaMapRepository(FakeStorage storage)
		{
			_storage = storage;
		}
		public void Add(IRtaMap entity)
		{
			_storage.Add(entity);
		}

		public void Remove(IRtaMap entity)
		{
			_storage.Remove(entity);
		}

		public IRtaMap Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IRtaMap> LoadAll()
		{
			return _storage.LoadAll<IRtaMap>().ToList();
		}

		public IRtaMap Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IRtaMap> LoadAllCompleteGraph()
		{
			return _storage.LoadAll<IRtaMap>().ToList();
		}	
	}
}