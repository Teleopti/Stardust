using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
    public class OvertimePreferences: IOvertimePreferences
    {
        public IScheduleTag ScheduleTag { get; set; }
        public IActivity SkillActivity { get; set; }
        public IMultiplicatorDefinitionSet OvertimeType { get; set; }
        public TimePeriod SelectedTimePeriod { get; set; }=new TimePeriod(1, 1);
		public TimePeriod SelectedSpecificTimePeriod { get; set; } = new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1).Add(TimeSpan.FromHours(10)));
		public bool AllowBreakMaxWorkPerWeek { get; set; }
        public bool AllowBreakNightlyRest { get; set; }
        public bool AllowBreakWeeklyRest { get; set; }
        public bool AvailableAgentsOnly { get; set; }

	    public IRuleSetBag ShiftBagToUse { get; set; }
		public UseSkills UseSkills { get; set; }
    }
}