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
        bool AllowBreakMaxWorkPerWeek { get; set; }
        bool AllowBreakNightlyRest { get; set; }
        bool AllowBreakWeeklyRest { get; set; }
		  bool AvailableAgentsOnly { get; set; }
    }
}