using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Wfm.Adherence.Configuration;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeRtaRuleRepository : IRtaRuleRepository
	{
		private readonly IFakeStorage _storage;

		public FakeRtaRuleRepository(IFakeStorage storage)
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

		public IEnumerable<IRtaRule> LoadAll()
		{
			return _storage.LoadAll<IRtaRule>().ToList();
		}

		public IRtaRule Load(Guid id)
		{
			throw new NotImplementedException();
		}
	}
}