﻿namespace Teleopti.Ccc.Domain.FeatureFlags
{
	public enum Toggles
	{
		//Don't remove this one - used in tests
		TestToggle,
		//////
		// ReSharper disable InconsistentNaming
		Forecast_CopySettingsToWorkflow_11112,

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

		MessageBroker_SchedulingScreenMailbox_32733,

		MyTeam_MoveActivity_25206,
		MyTeam_StaffingMetrics_25562,
		MyTeam_Reports_31070,
		MyTeam_AbsenceBackToWork_31478,

		RTA_NotifyViaSMS_31567,

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
		MyTimeWeb_PreferenceShowNightViolation_33152,
		MyTimeWeb_JalaaliCalendar_32997,
		MyTimeWeb_AutoShiftTradeWithMeetingAndPersonalActivity_33281,


		Backlog_Module_23980,

		ETL_SpeedUpETL_30791,
		ETL_OnlyLatestQueueAgentStatistics_30787,

		Gamification_NewBadgeCalculation_31185,
		Portal_DifferentiateBadgeSettingForAgents_31318,

		SeatPlanner_Logon_32003,
		MyTeam_MakeTeamScheduleConsistent_31897,

		Wfm_MinimumScaffolding_32659,
		Wfm_ResourcePlanner_32892,
		Wfm_ChangePlanningPeriod_33043,
		WfmPeople_AdvancedSearch_32973,
		Wfm_Outbound_Campaign_32696,
		WfmForecast_QueueStatistics_32572,
		WfmForecast_ResultView_33605,
		WfmForecast_MethodsComparisonView_33610,

		Scheduler_OptimizeFlexibleDayOffs_22409,

		Tenant_RemoveNhibFiles_33685
		// ReSharper restore InconsistentNaming
	}
}