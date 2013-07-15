using System;
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
        public bool DoNotBreakMaxWorkPerWeek { get; set; }
        public bool DoNotBreakNightlyRest { get; set; }
        public bool DoNotBreakWeeklyRest { get; set; }
    }
}