using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class RuleSetDeletedActivityChecker
    {
        public bool ContainsDeletedActivity(IWorkShiftRuleSet ruleSet)
        {
            if(ruleSet == null)
                throw new ArgumentNullException(nameof(ruleSet));

            if (ruleSet.TemplateGenerator.BaseActivity.IsDeleted)
                return true;
            foreach (var workShiftExtender in ruleSet.ExtenderCollection)
            {
                if (workShiftExtender.ExtendWithActivity.IsDeleted)
                    return true;
            }
            foreach (var workShiftLimiter in ruleSet.LimiterCollection)
            {
                var activityLimiter = workShiftLimiter as ActivityTimeLimiter;
                if (activityLimiter != null && activityLimiter.Activity.IsDeleted)
                    return true;
            }
            return false;
        }
    }
}