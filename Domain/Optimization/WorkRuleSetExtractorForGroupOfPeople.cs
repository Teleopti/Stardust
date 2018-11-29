using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class WorkRuleSetExtractorForGroupOfPeople : IWorkRuleSetExtractorForGroupOfPeople
    {
        private readonly IEnumerable<IPerson> _persons;

        public WorkRuleSetExtractorForGroupOfPeople(IEnumerable<IPerson> persons)
        {
            _persons = persons;
        }

        public IEnumerable<IWorkShiftRuleSet> ExtractRuleSets(DateOnlyPeriod period)
        {
            return extractRuleSetBags(period).SelectMany(b => b.RuleSetCollection).Distinct().ToArray();
        }

        private IEnumerable<IRuleSetBag> extractRuleSetBags(DateOnlyPeriod period)
        {
			return _persons.SelectMany(person => person.PersonPeriods(period)).Select(pp => pp.RuleSetBag).Where(r => r != null)
				.Distinct();
        }
    }
}