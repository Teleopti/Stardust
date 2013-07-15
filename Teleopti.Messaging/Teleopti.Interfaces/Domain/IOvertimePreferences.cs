using System;

namespace Teleopti.Interfaces.Domain
{
    public interface IOvertimePreferences 
    {
        IScheduleTag ScheduleTag { get; set; }
        bool ExtendExistingShift { get; set; }
        IActivity SkillActivity { get; set; }
        IMultiplicatorDefinitionSet OvertimeType { get; set; }
        TimePeriod SelectedTimePeriod { get; set; }
        bool DoNotBreakMaxWorkPerWeek { get; set; }
        bool DoNotBreakNightlyRest { get; set; }
        bool DoNotBreakWeeklyRest { get; set; }
    }
}