namespace Teleopti.Ccc.Domain.FeatureFlags
{
	public enum Toggles
	{
		//Don't remove this one - used in tests
		TestToggle,
		//////
		// ReSharper disable InconsistentNaming
		Scheduler_Seniority_24331,
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
		Scheduler_DoNotBreakMinWeekWorkTimeOptimization_31992,

		Preference_PreferenceAlertWhenMinOrMaxHoursBroken_25635,
		MyReport_AgentQueueMetrics_22254,
		Request_ShiftTradeRequestForMoreDays_20918,
		Request_GiveCommentWhenDenyOrApproveShiftTradeRequest_28341,
		Request_SeePossibleShiftTradesFromAllTeams_28770,
		Request_FilterPossibleShiftTradeByTime_24560,
		Settings_SetAgentDescription_23257,
		Settings_AlertViaEmailFromSMSLink_30444,

		RTA_SeePercentageAdherenceForOneAgent_30783,
		RTA_SeeAdherenceDetailsForOneAgent_31285,
		RTA_HangfireEventProcessing_31237,
		RTA_NoBroker_31237,
		RTA_EventStreamInitialization_31237,
		RTA_CalculatePercentageInAgentTimezone_31236,
		RTA_NeutralAdherence_30930,

		MyTeam_MoveActivity_25206,
		MyTeam_StaffingMetrics_25562,
		MyTeam_Reports_31070,
		MyTeam_AbsenceBackToWork_31478,

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
		MyTimeWeb_AnonymousTrades_31638,
		MyTimeWeb_LockTrades_31637,
		MyTimeWeb_AvailabilityVerifyHours_31654,
		MyTimeWeb_OrganisationalBasedLeaderboard_31184,
		MyTimeWeb_TradeWithDayOffAndEmptyDay_31317,
		MyTimeWeb_SortSchedule_32092,
		MyTimeWeb_EnhanceTeamSchedule_32580,

		Messaging_HttpSender_29205,

		Backlog_Module_23980,

		ETL_SpeedUpETL_30791,
		ETL_OnlyLatestQueueAgentStatistics_30787,

		Gamification_NewBadgeCalculation_31185,
		Portal_DifferentiateBadgeSettingForAgents_31318,

		SeatPlanner_Logon_32003,
		MyTeam_MakeTeamScheduleConsistent_31897,
		MultiTenantSSOSupport_StandardReports_15093,
		MultiTenancy_SDK_17458,
		MultiTenancy_People_32113,
		Wfm_MinimumScaffolding_32659,
		Wfm_ResourcePlanner_32892
		// ReSharper restore InconsistentNaming
	}
}