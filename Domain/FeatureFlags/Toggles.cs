namespace Teleopti.Ccc.Domain.FeatureFlags
{
	public enum Toggles
	{
		//Don't remove these one - used in tests
		EnabledFeature,
		DisabledFeature,
		//////
		
		WeeklyRestRuleSolver,
		TeamBlockMoveTimeBetweenDays,
		//Show last updated states of realtime adherence overview
		RtaLastStatesOverview,
		PreferenceAlertWhenMinOrMaxHoursBroken
	}
}