using System;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public interface IRuleSetDeletedActivityChecker
    {
        bool ContainsDeletedActivity(IWorkShiftRuleSet ruleSet);
    }

    public class RuleSetDeletedActivityChecker : IRuleSetDeletedActivityChecker
    {
        public bool ContainsDeletedActivity(IWorkShiftRuleSet ruleSet)
        {
            if(ruleSet == null)
                throw new ArgumentNullException("ruleSet");

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