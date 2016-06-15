﻿namespace Teleopti.Ccc.Domain.FeatureFlags
{
	//Yes, yes, I know. This should be named "Toggle" but too many name collisions
	//with namespaces if changed now.
	public enum Toggles
	{
		//Don't remove this one - used in tests
		TestToggle,
		TestToggle2,
		TestToggle3,
		//////
		// ReSharper disable InconsistentNaming
		Forecast_CopySettingsToWorkflow_11112,

		Preference_PreferenceAlertWhenMinOrMaxHoursBroken_25635,
		Request_RecalculatePersonAccountBalanceOnRequestConsumer_36850,
		Settings_SetAgentDescription_23257,
		Settings_AlertViaEmailFromSMSLink_30444,

		RTA_AdherenceDetails_34267,
		RTA_ScheduleProjectionReadOnlyHangfire_35703,
		RTA_AlarmContext_29357,
        RTA_TotalOutOfAdherenceTime_38702,
		RTA_PauseButton_39144,
		RTA_MonitorBySkills_39081,

		MessageBroker_SchedulingScreenMailbox_32733,

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
		WfmIntraday_MonitorActualvsForecasted_35176,
		WfmTeamSchedule_AbsenceReporting_35995,
		WfmTeamSchedule_SetAgentsPerPage_36230,
		WfmTeamSchedule_SwapShifts_36231,
		WfmTeamSchedule_SeeScheduleChangesByOthers_36303,
		WfmTeamSchedule_RemoveAbsence_36705,
		WfmTeamSchedule_AddActivity_37541,
		WfmTeamSchedule_RemoveActivity_37743,
		WfmTeamSchedule_PrepareForRelease_37752,
		WfmTeamSchedule_MoveActivity_37744,
		WfmTeamSchedule_ShowContractTime_38509,
		WfmTeamSchedule_AddPersonalActivity_37742,
		WebByPassDefaultPermissionCheck_37984,
		WfmTeamSchedule_RevertToPreviousSchedule_39002,

		WfmReportPortal_Basic_38825,

		Wfm_Requests_Basic_35986,
		Wfm_Requests_People_Search_36294,
		Wfm_Requests_Performance_36295,
		Wfm_Requests_ApproveDeny_36297,
		Wfm_Requests_Waitlist_36232,
		Wfm_Requests_Filtering_37748,
		Wfm_Requests_Cancel_37741,
		Wfm_Requests_Cancel_Agent_38055,
		Wfm_Requests_Run_waitlist_38071,
		Wfm_Requests_ShiftTrade_37751,
		Wfm_Requests_PrepareForRelease_38771,

		Wfm_Intraday_38074,

		Wfm_People_PrepareForRelease_39040,

		Report_UseOpenXmlFormat_35797,
		Scheduler_IntradayOptimization_36617,
		Scheduler_AgentSkillAnalyzer_12345,
		Wfm_Use_Stardust,
		Wfm_ForecastFileImportOnStardust_37047,

		ETL_SpeedUpPersonPeriodIntraday_37162_37439,
		ETL_SpeedUpGroupPagePersonIntraday_37623,
		WfmGlobalLayout_personalOptions_37114,

		ResourcePlanner_TeamBlockDayOffForIndividuals_37998,
		ResourcePlanner_ScheduleOvertimeOnNonWorkingDays_38025,
		ResourcePlanner_CascadingSkills_38524,

		Wfm_QuickForecastOnHangfire_35845,
		ETL_SpeedUpPermissionReport_33584,
		Wfm_RecalculateForecastOnHangfire_37971,
		ETL_SpeedUpFactScheduleNightly_38019,
		ETL_SpeedUpPersonPeriodNightly_38097,
		Stardust_MoveAbsenceRequestTo_37941,
		ETL_SpeedUpNightlyReloadDatamart_38131,
		ETL_SpeedUpIntradayPreference_37124,
		ETL_SpeedUpIntradaySkill_37543,
		Wfm_MoveNewAbsenceReportOnHangfire_38203,
		ETL_SpeedUpNightlySkill_37543,
		ShiftTrade_ToHangfire_38181,
		GroupPageCollection_ToHangfire_38178,
		SettingsForPersonPeriodChanged_ToHangfire_38207,
		ETL_SpeedUpIntradayDayOff_38213,
		Wfm_MoveExportMultiSiteSkillOnStardust_38134,
		Payroll_ToStardust_38204,
		ETL_SpeedUpScenario_38300,
		ETL_SpeedUpScenarioNightly_38300,
		Scheduler_LoadWithoutRequests_38567,
		PersonCollectionChanged_ToHangfire_38420,
		ETL_SpeedUpIntradayActivity_38303,
		ETL_SpeedUpNightlyActivity_38303,
		ETL_SpeedUpIntradayOvertime_38304,
		ETL_SpeedUpNightlyOvertime_38304,
		ETL_SpeedUpIntradayAbsence_38301,
		ETL_SpeedUpNightlyAbsence_38301,
		ETL_SpeedUpIntradayShiftCategory_38718,
		ETL_MoveBadgeCalculationToETL_38421,
		ETL_SpeedUpNightlyShiftCategory_38718,
        ETL_SpeedUpNightlyPreference_38283,
		ETL_SpeedUpIntradayRequest_38914,
		ETL_FasterIndexMaintenance_38847,
        ETL_SpeedUpIntradayScorecard_38933,
		ETL_SpeedUpIntradayAvailability_38926,
		ETL_SpeedUpNightlyAvailability_38926,
		ETL_SpeedUpNightlyRequest_38914,
		ETL_SpeedUpGroupPagePersonNightly_37623,
		Intraday_ResourceCalculateReadModel_39200
		// ReSharper restore InconsistentNaming
	}
}