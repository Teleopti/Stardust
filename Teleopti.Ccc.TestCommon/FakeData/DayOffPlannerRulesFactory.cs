using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    public static class DayOffPlannerRulesFactory
    {
        public static IDayOffPlannerRules Create()
        {
            return new DayOffPlannerRules();
        }

        public static IDayOffPlannerRules CreateWithFalseDefaultValues()
        {
            IDayOffPlannerRules rules = Create();
            rules.UseConsecutiveDaysOff = false;
            rules.UseConsecutiveWorkdays = false;
            rules.UseDaysOffPerWeek = false;
            rules.KeepFreeWeekendDays = false;
            rules.KeepFreeWeekends = false;
            return rules;
        }

    }
}
