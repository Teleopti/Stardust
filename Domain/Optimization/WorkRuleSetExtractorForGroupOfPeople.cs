using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

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
            var extractedWorkShiftRuleSets = new HashSet<IWorkShiftRuleSet>();

            IEnumerable<IRuleSetBag> extractedRuleSetBags = extractRuleSetBags(period);

            foreach (IRuleSetBag ruleSetBag in extractedRuleSetBags)
            {
                foreach (IWorkShiftRuleSet workShiftRuleSet in ruleSetBag.RuleSetCollection)
                {
                    extractedWorkShiftRuleSets.Add(workShiftRuleSet);
                } 
            }
            return extractedWorkShiftRuleSets.ToList();
        }

        private IEnumerable<IRuleSetBag> extractRuleSetBags(DateOnlyPeriod period)
        {
            var extractedRuleSetBag = new HashSet<IRuleSetBag>();

            foreach (IPerson person in _persons)
            {
                IList<IPersonPeriod> validPeriods = person.PersonPeriods(period);
                if (validPeriods.Count == 0)
                    continue;
                foreach (IPersonPeriod personPeriod in validPeriods)
                {
					if (personPeriod.RuleSetBag != null)
						extractedRuleSetBag.Add(personPeriod.RuleSetBag);
                }
            }
            return extractedRuleSetBag.ToList();
        }
    }
}