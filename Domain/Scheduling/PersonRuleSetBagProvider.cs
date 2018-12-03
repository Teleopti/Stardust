using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class PersonRuleSetBagProvider : IPersonRuleSetBagProvider
	{
		private readonly IRuleSetBagRepository _repository;

		public PersonRuleSetBagProvider(IRuleSetBagRepository repository)
		{
			_repository = repository;
		}

		public IRuleSetBag ForDate(IPerson person, DateOnly date)
		{
			var personPeriod = person?.Period(date);
			var ruleSetBag = personPeriod?.RuleSetBag;
			return ruleSetBag != null ? _repository.FindWithRuleSetsAndAccessibility(ruleSetBag.Id.GetValueOrDefault()) : null;
		}

		public IDictionary<DateOnly, IRuleSetBag> ForPeriod(IPerson person, DateOnlyPeriod period)
		{
			var allRuleSetBagIds = period.DayCollection().ToDictionary(k => k, v =>
			{
				var personPeriod = person?.Period(v);
				var ruleSetBag = personPeriod?.RuleSetBag;
				return ruleSetBag?.Id;
			});

			var ruleBagIdList = new HashSet<Guid>(allRuleSetBagIds.Values.Where(v => v.HasValue).Select(v => v.Value));
			var allRuleSetBags = (ruleBagIdList.IsEmpty()
				? Enumerable.Empty<IRuleSetBag>()
				: _repository.FindWithRuleSetsAndAccessibility(ruleBagIdList.ToArray())).ToLookup(b => b.Id);

			return allRuleSetBagIds.ToDictionary(item => item.Key, item => allRuleSetBags[item.Value].SingleOrDefault());
		}
	}
}