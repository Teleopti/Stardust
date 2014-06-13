namespace Teleopti.Ccc.Domain.FeatureFlags
{
	public enum Toggles
	{
		//Don't remove this one - used in tests
		TestToggle,
		//////
		TeamBlue_Seniority_Temporay,
		Forecast_CopySettingsToWorkflow,

		Scheduler_WeeklyRestRuleSolver_27108,
		Scheduler_TeamBlockMoveTimeBetweenDays_22407,
		Scheduler_TeamBlockAdhereWithMaxSeatRule_23419,
		Preference_PreferenceAlertWhenMinOrMaxHoursBroken_25635,
		MyReport_AgentQueueMetrics_22254,

		RTA_RtaLastStatesOverview_27789,
		RTA_DrilldownToAllAgentsInOneTeam_25234,

		MyTeam_MoveActivity_25206
	}
}