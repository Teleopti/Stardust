namespace Teleopti.Ccc.Domain.FeatureFlags
{
	public enum Toggles
	{
		//Don't remove this one - used in tests
		TestToggle,
		//////
		Scheduler_Seniority_11111,
		Forecast_CopySettingsToWorkflow_11112,

		Scheduler_WeeklyRestRuleSolver_27108,
		Scheduler_TeamBlockMoveTimeBetweenDays_22407,
		Scheduler_TeamBlockAdhereWithMaxSeatRule_23419,
		Preference_PreferenceAlertWhenMinOrMaxHoursBroken_25635,
		MyReport_AgentQueueMetrics_22254,
		Request_ShiftTradeRequestForMoreDays_20918,
		Request_GiveCommentWhenDenyOrApproveShiftTradeRequest_28341,
		Request_SeePossibleShiftTradesFromAllTeams_28770,
		Request_FilterPossibleShiftTradeByTime_24560,

		RTA_RtaLastStatesOverview_27789,
		RTA_DrilldownToAllAgentsInOneTeam_25234,
		RTA_MonitorMultipleBusinessUnits_28348,

		MyTeam_MoveActivity_25206,
		MyTeam_StaffingMetrics_25562,
        Scheduler_ShowIntadayESLWithShrinkage_21874,
		RTA_ViewAgentsForMultipleTeams_28967,

		MyTimeWeb_AgentBadge_28913
	}
}