using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeRuleSetBagRepository : IRuleSetBagRepository
	{
		private IList<IRuleSetBag> _ruleSetBags;

		public FakeRuleSetBagRepository()
		{
			_ruleSetBags = new List<IRuleSetBag>();
		}

		public void Add(IRuleSetBag root)
		{
			throw new NotImplementedException();
		}

		public IRuleSetBag Has(IRuleSetBag ruleSetBag)
		{
			_ruleSetBags.Add(ruleSetBag);
			return ruleSetBag;
		}

		public void Remove(IRuleSetBag root)
		{
			throw new NotImplementedException();
		}

		public IRuleSetBag Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IRuleSetBag> LoadAll()
		{
			return _ruleSetBags;
		}

		public IRuleSetBag Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IRuleSetBag> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; }
		public IEnumerable<IRuleSetBag> LoadAllWithRuleSets()
		{
			throw new NotImplementedException();
		}

		public IRuleSetBag Find(Guid id)
		{
			throw new NotImplementedException();
		}
	}
}