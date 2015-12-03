using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeRtaRuleRepository : IRtaRuleRepository
	{
		private readonly IList<IRtaRule> _data = new List<IRtaRule>();

		public void Add(IRtaRule entity)
		{
			_data.Add(entity);
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
			return _data;
		}

		public IRtaRule Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IRtaRule> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork
		{
			get
			{
				throw new NotImplementedException();
			}
		}
	}
}