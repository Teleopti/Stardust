using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeRuleSetBagRepository : IRuleSetBagRepository
	{
		private readonly IList<IRuleSetBag> _ruleSetBags = new List<IRuleSetBag>();
		
		public void Add(IRuleSetBag root)
		{
			_ruleSetBags.Add(root);
		}

		public IRuleSetBag Has(IRuleSetBag ruleSetBag)
		{
			_ruleSetBags.Add(ruleSetBag);
			return ruleSetBag;
		}

		public void Remove(IRuleSetBag root)
		{
			_ruleSetBags.Remove(root);
		}

		public IRuleSetBag Get(Guid id)
		{
			return _ruleSetBags.FirstOrDefault(r => r.Id == id);
		}

		public IList<IRuleSetBag> LoadAll()
		{
			return _ruleSetBags;
		}

		public IRuleSetBag Load(Guid id)
		{
			return _ruleSetBags.FirstOrDefault(r => r.Id == id);
		}

		public IUnitOfWork UnitOfWork { get; }
		public IEnumerable<IRuleSetBag> LoadAllWithRuleSets()
		{
			return _ruleSetBags.Where(r => r.RuleSetCollection.Any());
		}

		public IRuleSetBag FindWithRuleSetsAndAccessibility(Guid id)
		{
			return _ruleSetBags.FirstOrDefault(r => r.Id == id);
		}

		public IRuleSetBag[] FindWithRuleSetsAndAccessibility(Guid[] ruleBagIds)
		{
			return _ruleSetBags.Where(r => ruleBagIds.Contains(r.Id.Value)).ToArray();
		}
	}
}