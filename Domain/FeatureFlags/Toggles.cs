﻿namespace Teleopti.Ccc.Domain.FeatureFlags
{
	//Yes, yes, I know. This should be named "Toggle" but too many name collisions
	//with namespaces if changed now.
	public enum Toggles
	{
		//Don't remove this one - used in tests
		TestToggle,
		TestToggle2,
		//////
		// ReSharper disable InconsistentNaming
		Forecast_CopySettingsToWorkflow_11112,

		Preference_PreferenceAlertWhenMinOrMaxHoursBroken_25635,
		Request_RecalculatePersonAccountBalanceOnRequestConsumer_36850,
		Settings_SetAgentDescription_23257,
		Settings_AlertViaEmailFromSMSLink_30444,

		RTA_AdherenceDetails_34267,
		RTA_CalculatePercentageInAgentTimezone_31236,
		RTA_NeutralAdherence_30930,
		RTA_DeletedPersons_36041,
		RTA_TerminatedPersons_36042,
		RTA_TeamChanges_36043,
		RTA_ScaleOut_36979,

		MessageBroker_SchedulingScreenMailbox_32733,

		RTA_NotifyViaSMS_31567,

		Portal_NewLandingpage_29415,
		Show_StaticPageOnNoInternet_29415,
		MyTimeWeb_SortSchedule_32092,
		MyTimeWeb_EnhanceTeamSchedule_32580,
		MyTimeWeb_PreferenceShowNightViolation_33152,
		MyTimeWeb_JalaaliCalendar_32997,
		MyTimeWeb_KeepUrlAfterLogon_34762,
		MyTimeWeb_TeamScheduleNoReadModel_36210,
		MyTimeWeb_ShiftTradePossibleTradedSchedulesNoReadModel_36211,
		MyTimeWeb_ShiftTradeBoardNoReadModel_36662,

		Backlog_Module_23980,

		ETL_SpeedUpETL_30791,
		ETL_OnlyLatestQueueAgentStatistics_30787,
		Gamification_NewBadgeCalculation_31185,
		Portal_DifferentiateBadgeSettingForAgents_31318,

		SeatPlanner_Logon_32003,
		Wfm_SeatPlan_SeatMapBookingView_32814,
		MyTimeWeb_ShowSeatBooking_34799,
		MyTeam_MakeTeamScheduleConsistent_31897,

		Wfm_MinimumScaffolding_32659,
		Wfm_ResourcePlanner_32892,
		Wfm_ChangePlanningPeriod_33043,
		WfmPeople_AdvancedSearch_32973,
		WfmPeople_ImportUsers_33665,
		WfmPeople_AdjustSkill_34138,
		Wfm_Outbound_Campaign_32696,
		Wfm_Outbound_Campaign_GanttChart_Navigation_34924,
		WfmPermission_ReplaceOldPermission_34671,
		Wfm_RTA_34621,
		Wfm_RTA_ProperAlarm_34975,
		Wfm_RTA_ProperReleaseToggle_36750,
		WfmIntraday_MonitorActualvsForecasted_35176,
		WfmTeamSchedule_AbsenceReporting_35995,
		WfmTeamSchedule_SetAgentsPerPage_36230,
		WfmTeamSchedule_SwapShifts_36231,
		WfmTeamSchedule_SeeScheduleChangesByOthers_36303,
		WfmTeamSchedule_RemoveAbsence_36705,
		WfmTeamSchedule_AddActivity_37541,
		WfmTeamSchedule_RemoveActivity_37743,

		ResourcePlanner_WeeklyRestSolver_35043,

		Wfm_Requests_Basic_35986,
		Wfm_Requests_People_Search_36294,
		Wfm_Requests_Performance_36295,
		Wfm_Requests_ApproveDeny_36297,
		Wfm_Requests_Waitlist_36232,
		Wfm_Requests_Filtering_37748,
		Wfm_Requests_Cancel_37741,

		Report_UseOpenXmlFormat_35797,
		Scheduler_IntradayOptimization_36617,
		Scheduler_AgentSkillAnalyzer_12345,
		Wfm_Use_Stardust,
		Wfm_ForecastFileImportOnStardust_37047,

		ETL_SpeedUpPersonPeriodIntraday_37162_37439,
		ETL_SpeedUpGroupPagePersonIntraday_37623,
		WfmGlobalLayout_personalOptions_37114,

		ResourcePlanner_SkillGroupDeleteAfterCalculation_37048,
		ResourcePlanner_JumpOutWhenLargeGroupIsHalfOptimized_37049,
		ResourcePlanner_IntradayIslands_36939,
		ResourcePlanner_CascadingSkillsPOC_37679,
		ResourcePlanner_TeamBlockDayOffForIndividuals_37998,

		Wfm_QuickForecastOnHangfire_35845,
		ETL_SpeedUpPermissionReport_33584,
		Wfm_RecalculateForecastOnHangfire_37971,
		ETL_SpeedUpFactScheduleNightly_38019,
		ETL_SpeedUpPersonPeriodNightly_38097,
		Stardust_MoveAbsenceRequestTo_37941,
		ETL_SpeedUpNightlyReloadDatamart_38131,
		ETL_SpeedUpIntradayPreference_37124,
		// ReSharper restore InconsistentNaming
	}
}