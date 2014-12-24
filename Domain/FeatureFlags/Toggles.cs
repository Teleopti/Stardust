namespace Teleopti.Ccc.Domain.FeatureFlags
{
	public enum Toggles
	{
		//Don't remove this one - used in tests
		TestToggle,
		//////
		Scheduler_Seniority_11111,
		Forecast_CopySettingsToWorkflow_11112,

		Scheduler_AdvanceFilter_29555,
		Scheduler_WeeklyRestRuleSolver_27108,
		Scheduler_TeamBlockMoveTimeBetweenDays_22407,
		Scheduler_TeamBlockAdhereWithMaxSeatRule_23419,
		Scheduler_ShowIntadayESLWithShrinkage_21874,
		Scheduler_OccupancyESL_11724,
		Scheduler_HidePointsFairnessSystem_28317,
		Scheduler_SudentAvailabilityForFixedStaff_30393,
		Scheduler_BackToLegalShift_29831,
		Scheduler_IntraIntervalSolver_29845,
		Schedule_IntraIntervalOptimizer_29846,
		Schedule_PublishSchedules_30929,
		Schedule_OvertimeBeforeShiftStart_30712,

		Preference_PreferenceAlertWhenMinOrMaxHoursBroken_25635,
		MyReport_AgentQueueMetrics_22254,
		Request_ShiftTradeRequestForMoreDays_20918,
		Request_GiveCommentWhenDenyOrApproveShiftTradeRequest_28341,
		Request_SeePossibleShiftTradesFromAllTeams_28770,
		Request_FilterPossibleShiftTradeByTime_24560,
		Request_ShiftTradeWithEmptyDays_28926,
		Settings_SetAgentDescription_23257,
		Settings_AlertViaEmailFromSMSLink_30444,

		RTA_RtaLastStatesOverview_27789,
		RTA_DrilldownToAllAgentsInOneTeam_25234,
		RTA_MonitorMultipleBusinessUnits_28348,
		RTA_ChangeScheduleInAgentStateView_29934,
		RTA_SeePercentageAdherenceForOneAgent_30783,
		RTA_SeeAdherenceDetailsForOneAgent_31285,
		RTA_HangfireEventProcessing_31593,
		RTA_HangfireEventProcessinUsingMsmq_31593,

		MyTeam_MoveActivity_25206,
		MyTeam_StaffingMetrics_25562,
		MyTeam_Reports_31070,
		MyTeam_AbsenceBackToWork_31478,

		RTA_ViewAgentsForMultipleTeams_28967,
		RTA_NotifyViaSMS_31567,

		MyTimeWeb_AgentBadge_28913,
        MyTimeWeb_FullDayAbsenceConfiguration_30552,
		Portal_ResetBadges_30544,
		Badge_Leaderboard_30584,
		Portal_NewLandingpage_29415,
		Show_StaticPageOnNoInternet_29415,
		MyTimeWeb_AbsenceReport_31011,
		MyTimeWeb_ShiftTradeExchangeBulletin_31296,
		MyTimeWeb_SeeAnnouncedShifts_31639,
		MyTimeWeb_AvailabilityVerifyHours_31654,
		MyTimeWeb_OrganisationalBasedLeaderboard_31184,

		Messaging_HttpSender_29205,

		ETL_SpeedUpETL_30791,
		RTA_NoBroker_31237,
		Gamification_NewBadgeCalculation_31185
	}
}