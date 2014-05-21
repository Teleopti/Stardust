namespace Teleopti.Ccc.Domain.FeatureFlags
{
	public enum Toggles
	{
		//Don't remove these one - used in tests
		EnabledFeature,
		DisabledFeature,
		//////

		Scheduler_WeeklyRestRuleSolver_27108,

		Scheduler_TeamBlockMoveTimeBetweenDays_22407,

		RTA_RtaLastStatesOverview_27789,
		RTA_DrilldownToAllAgentsInOneTeam_25234,

		ModuleName_PreferenceAlertWhenMinOrMaxHoursBroken_66666
	}
}