using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeRtaRuleRepository : IRtaRuleRepository
	{
		private readonly FakeStorage _storage;

		public FakeRtaRuleRepository(FakeStorage storage)
		{
			_storage = storage;
		}
		public void Add(IRtaRule entity)
		{
			_storage.Add(entity);
		}

		public void Remove(IRtaRule entity)
		{
			throw new NotImplementedException();
		}

		public IRtaRule Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IRtaRule> LoadAll()
		{
			return _storage.LoadAll<IRtaRule>().ToList();
		}

		public IRtaRule Load(Guid id)
		{
			throw new NotImplementedException();
		}
	}
}