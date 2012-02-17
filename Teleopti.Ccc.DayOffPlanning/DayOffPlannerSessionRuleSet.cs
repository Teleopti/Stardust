using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanning
{
    public interface IDayOffPlannerSessionRuleSet
    {
        MinMax<int> DaysOffPerWeek { get; set; }
        MinMax<int> ConsecutiveDaysOff { get; set; }
        MinMax<int> ConsecutiveWorkdays { get; set; }
        MinMax<int> FreeWeekends { get; set; }
        MinMax<int> FreeWeekendDays { get; set; }
        bool UseDaysOffPerWeek { get; set; }
        bool UseConsecutiveDaysOff { get; set; }
        bool UseConsecutiveWorkdays { get; set; }
        bool UseFreeWeekends { get; set; }
        bool UseFreeWeekendDays { get; set; }
        bool ConsiderWeekBefore { get; set; }
        bool ConsiderWeekAfter { get; set; }
    }

    public class DayOffPlannerSessionRuleSet : IDayOffPlannerSessionRuleSet
    {
        public MinMax<int> DaysOffPerWeek { get; set; }
        public MinMax<int> ConsecutiveDaysOff { get; set; }
        public MinMax<int> ConsecutiveWorkdays { get; set; }
        public MinMax<int> FreeWeekends { get; set; }
        public MinMax<int> FreeWeekendDays { get; set; }
        public bool UseDaysOffPerWeek { get; set; }
        public bool UseConsecutiveDaysOff { get; set; }
        public bool UseConsecutiveWorkdays { get; set; }
        public bool UseFreeWeekends { get; set; }
        public bool UseFreeWeekendDays { get; set; }
        public bool ConsiderWeekBefore { get; set; }
        public bool ConsiderWeekAfter { get; set; }

        public DayOffPlannerSessionRuleSet()
        {
            DaysOffPerWeek = new MinMax<int>(0, 100);
            ConsecutiveDaysOff = new MinMax<int>(0, 100);
            ConsecutiveWorkdays = new MinMax<int>(0, 100);
            FreeWeekends = new MinMax<int>(0, 100);
            FreeWeekendDays = new MinMax<int>(0, 100);
            UseDaysOffPerWeek = false;
            UseConsecutiveDaysOff = false;
            UseConsecutiveWorkdays = false;
            UseFreeWeekends = false;
            UseFreeWeekendDays = false;
            ConsiderWeekBefore = false;
            ConsiderWeekAfter = false;
        }
    }
}