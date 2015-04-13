using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class RuleSetBagsOfGroupOfPeopleCanHaveShortBreakLoader : IRuleSetBagsOfGroupOfPeopleCanHaveShortBreakLoader
    {
        public IWorkRuleSetExtractorForGroupOfPeople CreateWorkRuleSetExtractor(IEnumerable<IPerson> persons)
        {
            return new WorkRuleSetExtractorForGroupOfPeople(persons);
        }

        public IWorkShiftRuleSetCanHaveShortBreak CreateWorkShiftRuleSetCanHaveShortBreak(IEnumerable<IPerson> persons)
        {
            ITimePeriodCanHaveShortBreak timePeriodCanHaveShortBreak = CreateTimePeriodCanHaveShortBreak();
            ISkillExtractor personalSkillsExtractor = CreatePersonalSkillsExtractor(persons);
            return new WorkShiftRuleSetCanHaveShortBreak(timePeriodCanHaveShortBreak, personalSkillsExtractor);
        }

        public static ITimePeriodCanHaveShortBreak CreateTimePeriodCanHaveShortBreak()
        {
            return new TimePeriodCanHaveShortBreak();
        }

        public static ISkillExtractor CreatePersonalSkillsExtractor(IEnumerable<IPerson> persons)
        {
            return new SkillExtractorForGroupOfPeople(persons);
        }
    }
}