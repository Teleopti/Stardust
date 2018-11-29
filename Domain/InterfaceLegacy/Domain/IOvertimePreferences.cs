namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    public interface IOvertimePreferences 
    {
        IScheduleTag ScheduleTag { get; set; }
        IActivity SkillActivity { get; set; }
        IMultiplicatorDefinitionSet OvertimeType { get; set; }
        TimePeriod SelectedTimePeriod { get; set; }
		TimePeriod SelectedSpecificTimePeriod { get; set; }
        bool AllowBreakMaxWorkPerWeek { get; set; }
        bool AllowBreakNightlyRest { get; set; }
        bool AllowBreakWeeklyRest { get; set; }
        bool AvailableAgentsOnly { get; set; }
		IRuleSetBag ShiftBagToUse { get; set; }
	    UseSkills UseSkills { get; set; }
    }

	public enum UseSkills
	{
		All,
		Primary
	}
}