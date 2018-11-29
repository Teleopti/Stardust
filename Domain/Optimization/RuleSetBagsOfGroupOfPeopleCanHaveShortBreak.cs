using System.Collections.Generic;
using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class RuleSetBagsOfGroupOfPeopleCanHaveShortBreak
    {
        private readonly IRuleSetBagsOfGroupOfPeopleCanHaveShortBreakLoader _loader;
        
        public RuleSetBagsOfGroupOfPeopleCanHaveShortBreak(IRuleSetBagsOfGroupOfPeopleCanHaveShortBreakLoader loader)
        {
            _loader = loader;
        }

        public bool CanHaveShortBreak(IEnumerable<IPerson> persons, DateOnlyPeriod period)
        {
            IWorkRuleSetExtractorForGroupOfPeople workRuleSetExtractor = _loader.CreateWorkRuleSetExtractor(persons);
            IWorkShiftRuleSetCanHaveShortBreak workShiftRuleSetCanHaveShortBreak = _loader.CreateWorkShiftRuleSetCanHaveShortBreak(persons);

            IEnumerable<IWorkShiftRuleSet> workShiftRuleSets = workRuleSetExtractor.ExtractRuleSets(period);
            return ProceedWithCheckingWorkShiftRuleSets(workShiftRuleSets, workShiftRuleSetCanHaveShortBreak);
        }

        private static bool ProceedWithCheckingWorkShiftRuleSets(IEnumerable<IWorkShiftRuleSet> workShiftRuleSets, IWorkShiftRuleSetCanHaveShortBreak workShiftRuleSetCanHaveShortBreak)
        {
            foreach (IWorkShiftRuleSet workShiftRuleSet in workShiftRuleSets)
            {
                if(workShiftRuleSetCanHaveShortBreak.CanHaveShortBreak(workShiftRuleSet))
                {
                    ILog log = LogManager.GetLogger(typeof(WorkShiftRuleSet));
                    log.Info("RuleSet " + workShiftRuleSet.Description.Name + "is not safe for short break");
                    return true;
                }
                    
            }
            return false;
        }
    }
}