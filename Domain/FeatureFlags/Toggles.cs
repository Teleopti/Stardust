﻿namespace Teleopti.Ccc.Domain.FeatureFlags
{
	//Yes, yes, I know. This should be named "Toggle" but too many name collisions
	//with namespaces if changed now.
	public enum Toggles
	{
		//Don't remove these - used in tests
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
		RTA_HideAgentsByStateGroup_40469,
		RTA_FasterUpdateOfScheduleChanges_40536,
		RTA_SiteAndTeamOnSkillOverview_40817,
		RTA_SeeAllOutOfAdherencesToday_39146,

		Portal_NewLandingpage_29415,
		Show_StaticPageOnNoInternet_29415,
		MyTimeWeb_TeamScheduleNoReadModel_36210,
		MyTimeWeb_ShiftTradePossibleTradedSchedulesNoReadModel_36211,
		MyTimeWeb_ShiftTradeBoardNoReadModel_36662,
		MyTimeWeb_MakeRequestsResponsiveOnMobile_40266,
		MyTimeWeb_ShiftTradeFilterSite_40374,
		MyTimeWeb_ShiftTradeOptimization_40792,
		MyTimeWeb_PreferenceForMobile_40265,
		MyTimeWeb_ValidateAbsenceRequestsSynchronously_40747,

		Backlog_Module_23980,

		Gamification_NewBadgeCalculation_31185,

		MyTimeWeb_ShowSeatBookingMonthView_39068,

		Wfm_MinimumScaffolding_32659,
		Wfm_ResourcePlanner_32892,
		Wfm_ChangePlanningPeriod_33043,
		WfmPermission_ReplaceOldPermission_34671,
		WfmIntraday_MonitorActualvsForecasted_35176,
		WfmTeamSchedule_AbsenceReporting_35995,
		WfmTeamSchedule_SetAgentsPerPage_36230,
		WfmTeamSchedule_SwapShifts_36231,
		WfmTeamSchedule_SeeScheduleChangesByOthers_36303,
		WfmTeamSchedule_RemoveAbsence_36705,
		WfmTeamSchedule_AddActivity_37541,
		WfmTeamSchedule_RemoveActivity_37743,
		WfmTeamSchedule_MoveActivity_37744,
		WfmTeamSchedule_ShowContractTime_38509,
		WfmTeamSchedule_AddPersonalActivity_37742,
		WfmTeamSchedule_MoveInvalidOverlappedActivity_40688,
		WfmTeamSchedule_ShowNightlyRestWarning_39619,
		WfmTeamSchedule_ShowWeeklyWorktimeWarning_39799,
		WfmTeamSchedule_ShowWeeklyRestTimeWarning_39800,
		WfmTeamSchedule_ShowDayOffWarning_39801,
		WfmTeamSchedule_FilterValidationWarnings_40110,

		WfmTeamSchedule_ShowWarningForOverlappingCertainActivities_39938,
		WfmTeamSchedule_ShowOverwrittenLayerWarning_40109,
		WfmTeamSchedule_AutoMoveOverwrittenActivityForOperations_40279,
		WfmTeamSchedule_EditAndDisplayInternalNotes_40671,
		WfmTeamSchedule_CheckPersonalAccountWhenAddingAbsence_41088,

		WebByPassDefaultPermissionCheck_37984,
		WfmTeamSchedule_RevertToPreviousSchedule_39002,
		WfmTeamSchedule_ShowShiftCategory_39796,
		WfmTeamSchedule_ModifyShiftCategory_39797,
		WfmTeamSchedule_MakeNewMyTeamDefault_39744,
		WfmTeamSchedule_WeekView_39870,
		WfmTeamSchedule_MakePersonalActivityUnmerged_40252,
		WfmTeamSchedule_WeeklyContractTime_39871,
		WfmTeamSchedule_ShowScheduleBasedOnTimeZone_40925,
		WfmTeamSchedule_ShowShiftsForAgentsInDistantTimeZones_41305,


		WfmTeamSchedule_MoveToBaseLicense_41039,

		WfmReportPortal_Basic_38825,
		WfmReportPortal_LeaderBoard_39440,

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
		Wfm_Requests_ShiftTrade_More_Relevant_Information_38492,
		Wfm_Requests_PrepareForRelease_38771,
		Wfm_Requests_ApproveDeny_ShiftTrade_38494,
		Wfm_Requests_Default_Status_Filter_39472,
		Wfm_Requests_Show_Pending_Reasons_39473,
		Wfm_Requests_Approve_Based_On_Budget_Allotment_39626,
		Wfm_Requests_Reply_Message_39629,
		Wfm_Requests_Show_Personal_Account_39628,
		Wfm_Requests_Approve_Based_On_Intraday_39868,
		Wfm_Requests_Site_Open_Hours_39936,
		Wfm_Requests_Waitlist_Process_Order_39869,
		Wfm_Requests_Save_Grid_Columns_37976,
		Wfm_Requests_Check_Max_Seats_39937,
		Wfm_Requests_Check_Max_Seats_NoReadModel_39937,
		Wfm_Requests_Check_Expired_Requests_40274,

		Wfm_Intraday_38074,

		Wfm_People_PrepareForRelease_39040,

		Report_UseOpenXmlFormat_35797,
		Scheduler_IntradayOptimization_36617,
		Scheduler_AgentSkillAnalyzer_12345,
		Wfm_Use_Stardust,
		Wfm_ForecastFileImportOnStardust_37047,

		WfmGlobalLayout_personalOptions_37114,

		ResourcePlanner_TeamBlockDayOffForIndividuals_37998,
		ResourcePlanner_CascadingSkillsGUI_40018,
		ResourcePlanner_CalculateFarAwayTimeZones_40646,
		ResourcePlanner_FilterOvertimeAvail_37113,
		ResourcePlanner_NegativeOrderIndex_39062,
		ResourcePlanner_MaxSeatsNew_40939,
		ResourcePlanner_HideSkillPrioSliders_41312,

		Wfm_DisplayOnlineHelp_39402,

		Wfm_QuickForecastOnHangfire_35845,
		Wfm_RecalculateForecastOnHangfire_37971,
		ETL_SpeedUpFactScheduleNightly_38019,
		ETL_SpeedUpPersonPeriodNightly_38097,
		Stardust_MoveAbsenceRequestTo_37941,
		ETL_SpeedUpNightlyReloadDatamart_38131,
		Wfm_MoveNewAbsenceReportOnHangfire_38203,
		ETL_SpeedUpNightlySkill_37543,
		ShiftTrade_ToHangfire_38181,
		Wfm_MoveExportMultiSiteSkillOnStardust_38134,
		Payroll_ToStardust_38204,
		ETL_SpeedUpScenarioNightly_38300,
		Scheduler_LoadWithoutRequests_38567,
		ETL_SpeedUpNightlyActivity_38303,
		ETL_SpeedUpNightlyOvertime_38304,
		ETL_SpeedUpNightlyAbsence_38301,
		ETL_MoveBadgeCalculationToETL_38421,
		ETL_SpeedUpNightlyShiftCategory_38718,
		ETL_SpeedUpNightlyPreference_38283,
		ETL_FasterIndexMaintenance_38847,
		ETL_SpeedUpNightlyAvailability_38926,
		ETL_SpeedUpNightlyRequest_38914,
		ETL_SpeedUpGroupPagePersonNightly_37623,
		Intraday_ResourceCalculateReadModel_39200,
		ETL_SpeedUpNightlyWorkload_38928,
		ETL_SpeedUpNightlyForecastWorkload_38929,
		AddActivity_TriggerResourceCalculation_39346,
		HealthCheck_ValidateReadModelPersonScheduleDay_39421,
		HealthCheck_ValidateReadModelScheduleDay_39423,
		HealthCheck_EasyValidateAndFixReadModels_39696,
		HealthCheck_ReinitializeReadModels_39697,

		ETL_SpeedUpIntradayBusinessUnit_38932,
		ETL_SpeedUpNightlyBusinessUnit_38932,

		AbsenceRequests_UseMultiRequestProcessing_39960,
		AbsenceRequests_SpeedupIntradayRequests_40754,
		AbsenceRequests_SpeedupEndToEnd_41384,
		ETL_EventbasedDate_39562,
		ReadModel_ToHangfire_39147,
		NewPasswordHash_40460,
		Wfm_Intraday_OptimalStaffing_40921,
		Wfm_SkillPriorityRoutingGUI_39885,
		AbsenceRequest_WithOrWithoutShrinkage_41060,
		LastHandlers_ToHangfire_41203,
		ETL_EventbasedTimeZone_40870,
		ETL_RemoveTimeZoneAndDateNightly_40870,
		Stardust_RunMultipleNodes_41224

		// ReSharper restore InconsistentNaming
	}
}