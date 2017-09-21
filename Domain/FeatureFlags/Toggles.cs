namespace Teleopti.Ccc.Domain.FeatureFlags
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

		RTA_SpreadTransactionLocksStrategy_41823,
		RTA_EventPackagesOptimization_43924,
		RTA_AsyncOptimization_43924,
		RTA_RememberMyPartOfTheBusiness_39082,

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
		MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913,
		MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880,
		MyTimeWeb_PreferenceForJalaliCalendar_42965,
		MyTimeWeb_PreferencePerformanceForMultipleUsers_43322,
		MyTimeWeb_DayScheduleForStartPage_43446,
		MyTimeWeb_DayScheduleForStartPage_Command_44209,
		MyTimeWeb_ViewWFMAppGuide_43848,
		MyTimeWeb_CalculateOvertimeProbabilityByPrimarySkill_44686,
		MyTimeWeb_OvertimeRequest_44558,

		Backlog_Module_23980,

		Gamification_NewBadgeCalculation_31185,

		MyTimeWeb_ShowSeatBookingMonthView_39068,

		Wfm_MinimumScaffolding_32659,
		Wfm_ResourcePlanner_32892,
		Wfm_ChangePlanningPeriod_33043,
		WfmIntraday_MonitorActualvsForecasted_35176,
		Wfm_HideUnusedTeamsAndSites_42690,
		Wfm_SearchAgentBasedOnCorrectPeriod_44552,
		Wfm_AddMyTimeLink_45088,
		Wfm_GroupPages_45057,
		Wfm_FetchBusinessHierarchyFromReadModel_45275,

		WfmTeamSchedule_MoveOvertimeActivity_44888,
		WfmTeamSchedule_SortRows_45056,
		WfmTeamSchedule_DisplayAndEditPublicNote_44783,

		WebByPassDefaultPermissionCheck_37984,

		WfmReportPortal_Basic_38825,
		WfmReportPortal_LeaderBoard_39440,

		Wfm_Requests_Approve_Based_On_Budget_Allotment_39626,
		Wfm_Requests_Approve_Based_On_Intraday_39868,
		Wfm_Requests_Approve_Based_On_Minimum_Approval_Time_40274,
		Wfm_Requests_Configurable_BusinessRules_For_ShiftTrade_40770,
		Wfm_Requests_TriggerResourceCalculationFromGui_43129,
		Wfm_Requests_OvertimeRequestHandling_45177,
		Wfm_Requests_Refactoring_45470,
		Wfm_Requests_ProcessWaitlistBefore24hRequests_45767,
		Wfm_Requests_HandleFourteenDaysFast_43390,

		Wfm_Outbound_IgnoreScheduleForReplan_43752,

		Wfm_Intraday_38074,

		Wfm_People_PrepareForRelease_39040,
		Wfm_People_ImportAndCreateAgentFromFile_42528,
		Wfm_People_SetFallbackValuesForImporting_43289,
		Wfm_People_MoveImportJobToBackgroundService_43582,

		Report_UseOpenXmlFormat_35797,
		Scheduler_IntradayOptimization_36617,
		Scheduler_ShowSkillPrioLevels_41980,

		WfmGlobalLayout_personalOptions_37114,
		
		ResourcePlanner_HideSkillPrioSliders_41312,
		ResourcePlanner_RunPerfTestAsTeam_43537,
		ResourcePlanner_MergeTeamblockClassicScheduling_44289,
		ResourcePlanner_MergeTeamblockClassicIntraday_45508,
		ResourcePlanner_RetireKeepPercentageOfShifts_45688,
		ResourcePlanner_SpeedUpShiftsWithinDay_45694,

		Wfm_DisplayOnlineHelp_39402,

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
		ConfigQRCodeURLForMobileApps_43224,
		MobileApps_EnableMobileNotifications_44476,

		ETL_EventbasedDate_39562,
		NewPasswordHash_40460,
		Wfm_Intraday_OptimalStaffing_40921,
		Wfm_SkillPriorityRoutingGUI_39885,
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
		Wfm_Web_Intraday_Rta_As_first_Choice_42206,
		Landing_Page_Data_Protection_Question_35721,
		Wfm_ResourcePlanner_SchedulingOnStardust_42874,

		Staffing_Info_Configuration_44687,
		WfmStaffing_AddOvertime_42524,
		Reporting_Optional_Columns_42066,
		Staffing_ReadModel_Keep8DaysHistoricalData_44652,
		ETL_SpeedUpNightlyDayOff_38213,
		Wfm_Intraday_SupportSkillTypeWebChat_42591,
		MyTimeWeb_MobileResponsive_43826,
		Staffing_ReadModel_BetterAccuracy_Step3_44331,
		Staffing_ReadModel_BetterAccuracy_Step4_43389,
		Wfm_Intraday_SupportSkillTypeEmail_44002,
		WFM_Intraday_Show_For_Other_Days_43504,
		WFM_Intraday_SupportMultisiteSkill_43874,
		WFM_Intraday_Export_To_Excel_44892,
		WFM_Forecaster_Refact_44480,
		WFM_Export_Forecast_44716,
		WFM_RedirectPermissionToWeb_44562,
		Staffing_BPOExchangeImport_45202,
		WFM_TrainingPlanner_44780,
		Report_Remove_Realtime_Scheduled_Time_Per_Activity_45560,
		Report_Remove_Realtime_Scheduled_Time_vs_Target_45559,
		Report_Remove_Scheduled_Time_Per_Activity_From_Scheduler_45640,
		Wfm_Staffing_45411, //this is to enable staffing

		WFM_WebScheduling_LowPriority_44320,
		WFM_Intraday_SupportOtherSkillsLikeEmail_44026,
		WFM_Unified_Skill_Group_Management_45417,
		WFM_AuditTrail_44006,
		Report_Show_Utc_In_Report_Selection_When_In_Use_45079
		// ReSharper restore InconsistentNaming
	}
}