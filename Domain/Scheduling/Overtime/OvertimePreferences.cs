using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
    public class OvertimePreferences: IOvertimePreferences
    {
        public IScheduleTag ScheduleTag { get; set; }
        public bool ExtendExistingShift { get; set; }
        public IActivity SkillActivity { get; set; }
        public IMultiplicatorDefinitionSet OvertimeType { get; set; }
        public TimePeriod SelectedTimePeriod { get; set; }
        public bool AllowBreakMaxWorkPerWeek { get; set; }
        public bool AllowBreakNightlyRest { get; set; }
        public bool AllowBreakWeeklyRest { get; set; }
        public bool AvailableAgentsOnly { get; set; }
    }
}