using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IDayOffPlannerRuleSetCreator
    {
        IDayOffPlannerSessionRuleSet CreateDayOffPlannerSessionRuleSet(IDaysOffPreferences daysOffPreferences);
    }

    public class DayOffPlannerRuleSetCreator : IDayOffPlannerRuleSetCreator
    {
        public IDayOffPlannerSessionRuleSet CreateDayOffPlannerSessionRuleSet(IDaysOffPreferences daysOffPreferences)
        {
            var dayOffPlannerSessionRuleSet = 
                new DayOffPlannerSessionRuleSet
                {
                    UseDaysOffPerWeek = daysOffPreferences.UseDaysOffPerWeek,
                    DaysOffPerWeek = daysOffPreferences.DaysOffPerWeekValue,
                    UseConsecutiveDaysOff = daysOffPreferences.UseConsecutiveDaysOff,
                    ConsecutiveDaysOff = daysOffPreferences.ConsecutiveDaysOffValue,
                    UseConsecutiveWorkdays = daysOffPreferences.UseConsecutiveWorkdays,
                    ConsecutiveWorkdays = daysOffPreferences.ConsecutiveWorkdaysValue,
                    UseFreeWeekendDays = daysOffPreferences.UseWeekEndDaysOff,
                    FreeWeekendDays = daysOffPreferences.WeekEndDaysOffValue,
                    UseFreeWeekends = daysOffPreferences.UseFullWeekendsOff,
                    FreeWeekends = daysOffPreferences.FullWeekendsOffValue,
                    ConsiderWeekBefore = daysOffPreferences.ConsiderWeekBefore,
                    ConsiderWeekAfter = daysOffPreferences.ConsiderWeekAfter
                };

            return dayOffPlannerSessionRuleSet;
        }
    }
}
