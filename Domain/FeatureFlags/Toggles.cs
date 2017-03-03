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
		
		RTA_SeeAllOutOfAdherencesToday_39146,
		RTA_SpreadTransactionLocksStrategy_41823,
		RTA_FasterAgentsView_42039,
		RTA_MonitorAgentsInPermittedOrganizationOnly_40660,
		RTA_SolidProofWhenManagingAgentAdherence_39351,

		MyTimeWeb_TeamScheduleNoReadModel_36210,
		MyTimeWeb_ShiftTradePossibleTradedSchedulesNoReadModel_36211,
		MyTimeWeb_ShiftTradeBoardNoReadModel_36662,
		MyTimeWeb_MakeRequestsResponsiveOnMobile_40266,
		MyTimeWeb_ShiftTradeFilterSite_40374,
		MyTimeWeb_ShiftTradeOptimization_40792,
		MyTimeWeb_PreferenceForMobile_40265,
		MyTimeWeb_StudentAvailabilityForMobile_42498,
		MyTimeWeb_ValidateAbsenceRequestsSynchronously_40747,
		MyTimeWeb_ShowContractTime_41462,
		MyTimeWeb_SortRequestList_40711,
		MyTimeWeb_ViewIntradayStaffingProbability_41608,
		MyTimeWeb_PreferenceForJalaliCalendar_42965,

		Backlog_Module_23980,

		Gamification_NewBadgeCalculation_31185,

		MyTimeWeb_ShowSeatBookingMonthView_39068,

		Wfm_MinimumScaffolding_32659,
		Wfm_ResourcePlanner_32892,
		Wfm_ChangePlanningPeriod_33043,
		WfmPermission_ReplaceOldPermission_34671,
		WfmIntraday_MonitorActualvsForecasted_35176,

		WfmTeamSchedule_SetAgentsPerPage_36230,
		WfmTeamSchedule_SeeScheduleChangesByOthers_36303,
		WfmTeamSchedule_SaveFavoriteSearches_42073,
		WfmTeamSchedule_SaveFavoriteSearchesInWeekView_42576,
		WfmTeamSchedule_DisplayScheduleOnBusinessHierachy_41260,
		WfmTeamSchedule_DisplayWeekScheduleOnBusinessHierachy_42252,

		WfmTeamSchedule_AbsenceReporting_35995,
		WfmTeamSchedule_AddActivity_37541,
		WfmTeamSchedule_AddPersonalActivity_37742,
		WfmTeamSchedule_AddOvertime_41696,
		WfmTeamSchedule_MoveActivity_37744,
		WfmTeamSchedule_MoveInvalidOverlappedActivity_40688,
		WfmTeamSchedule_MoveEntireShift_41632,
		WfmTeamSchedule_SwapShifts_36231,
		WfmTeamSchedule_RemoveAbsence_36705,
		WfmTeamSchedule_RemoveActivity_37743,
		WfmTeamSchedule_RemoveOvertime_42481,
		WfmTeamSchedule_RevertToPreviousSchedule_39002,

		WfmTeamSchedule_ShowShiftCategory_39796,
		WfmTeamSchedule_ModifyShiftCategory_39797,
		WfmTeamSchedule_ShowContractTime_38509,
		WfmTeamSchedule_EditAndDisplayInternalNotes_40671,

		WfmTeamSchedule_WeekView_39870,
		WfmTeamSchedule_WeeklyContractTime_39871,
		WfmTeamSchedule_WeekView_OpenDayViewForShiftEditing_40557,

		WfmTeamSchedule_ShowNightlyRestWarning_39619,
		WfmTeamSchedule_ShowWeeklyWorktimeWarning_39799,
		WfmTeamSchedule_ShowWeeklyRestTimeWarning_39800,
		WfmTeamSchedule_ShowDayOffWarning_39801,
		WfmTeamSchedule_ShowOverwrittenLayerWarning_40109,
		WfmTeamSchedule_FilterValidationWarnings_40110,

		WfmTeamSchedule_ShowWarningForOverlappingCertainActivities_39938,
		WfmTeamSchedule_AutoMoveOverwrittenActivityForOperations_40279,
		WfmTeamSchedule_CheckPersonalAccountWhenAddingAbsence_41088,

		WfmTeamSchedule_MakePersonalActivityUnmerged_40252,
		WfmTeamSchedule_ShowScheduleBasedOnTimeZone_40925,
		WfmTeamSchedule_ShowShiftsForAgentsInDistantTimeZones_41305,

		WfmTeamSchedule_MoveToBaseLicense_41039,
		WfmTeamSchedule_MakeNewMyTeamDefault_39744,

		WebByPassDefaultPermissionCheck_37984,

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
		Wfm_Requests_Approve_Based_On_Minimum_Approval_Time_40274,
		Wfm_Requests_Configurable_BusinessRules_For_ShiftTrade_40770,
		Wfm_Requests_ApprovingModifyRequests_41930,
		wfm_Requests_ReadModelOldData_42046,
		Wfm_Requests_ShowBudgetAllowanceDetail_41945,
		Wfm_Requests_DisplayRequestsOnBusinessHierachy_42309,
		Wfm_Requests_Configuarable_ShiftTradeTargetTimeSpecification_42450,
		Wfm_Requests_SaveFavoriteSearches_42578,
		Wfm_Requests_TriggerResourceCalculationFromGui_43129,

		Wfm_Seatplan_UseDatePeriodForPlanning_42167,

		Wfm_Intraday_38074,

		Wfm_People_PrepareForRelease_39040,
		Wfm_People_ImportAndCreateAgentFromFile_42528,
		Wfm_People_SetFallbackValuesForImporting_43289,

		Report_UseOpenXmlFormat_35797,
		Scheduler_IntradayOptimization_36617,
		Scheduler_ShowSkillPrioLevels_41980,

		WfmGlobalLayout_personalOptions_37114,

		ResourcePlanner_TeamBlockDayOffForIndividuals_37998,
		ResourcePlanner_CascadingSkillsGUI_40018,
		ResourcePlanner_NegativeOrderIndex_39062,
		ResourcePlanner_MaxSeatsNew_40939,
		ResourcePlanner_HideSkillPrioSliders_41312,
		ResourcePlanner_LessPersonAssignmentUpdates_42159,
		ResourcePlanner_ShiftCategoryLimitations_42680,
		ResourcePlanner_IntradayNoDailyValueCheck_42767,
		ResourcePlanner_LoadingLessSchedules_42639,
		ResourcePlanner_NotShovelCorrectly_41763,

		Wfm_DisplayOnlineHelp_39402,

		Wfm_QuickForecastOnHangfire_35845,
		ETL_SpeedUpFactScheduleNightly_38019,
		ETL_SpeedUpPersonPeriodNightly_38097,
		ETL_SpeedUpNightlyReloadDatamart_38131,
		ETL_SpeedUpNightlySkill_37543,
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
		ETL_SpeedUpNightlyWorkload_38928,
		ETL_SpeedUpNightlyForecastWorkload_38929,
		HealthCheck_ValidateReadModelPersonScheduleDay_39421,
		HealthCheck_ValidateReadModelScheduleDay_39423,
		HealthCheck_EasyValidateAndFixReadModels_39696,
		HealthCheck_ReinitializeReadModels_39697,

		ETL_SpeedUpIntradayBusinessUnit_38932,
		ETL_SpeedUpNightlyBusinessUnit_38932,

		QRCodeForMobileApps_42695,
		
		ETL_EventbasedDate_39562,
		ReadModel_ToHangfire_39147,
		NewPasswordHash_40460,
		Wfm_Intraday_OptimalStaffing_40921,
		Wfm_SkillPriorityRoutingGUI_39885,
		LastHandlers_ToHangfire_41203,
		ETL_EventbasedTimeZone_40870,
		ETL_RemoveTimeZoneAndDateNightly_40870,
		ETL_EventBasedAgentNameDescription_41432,
		ETL_FixScheduleForPersonPeriod_41393,
		Wfm_ArchiveSchedule_41498,
		Wfm_ImportSchedule_41247,
		SchedulingScreen_BatchAgentValidation_41552,
		Wfm_Intraday_ScheduledStaffing_41476,
		Mailbox_Optimization_41900,
		No_UnitOfWork_Nesting_42175,
		Wfm_Intraday_ESL_41827,
		AbsenceRequests_ValidateAllAgentSkills_42392,
		WfmStaffing_AllowActions_42524,
		Staffing_ReadModel_UseSkillCombination_42663,
		Wfm_Web_Intraday_Rta_As_first_Choice_42206,
		Wfm_ResourcePlanner_AgentGroup_42871,
		Landing_Page_Data_Protection_Question_35721,
		StaffingActions_UseRealForecast_42663,
		Wfm_ResourcePlanner_SchedulingOnStardust_42874
		// ReSharper restore InconsistentNaming
	}
}