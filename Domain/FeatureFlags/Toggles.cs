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
		TestToggle4,

		//////
		// ReSharper disable InconsistentNaming
		Forecast_CopySettingsToWorkflow_11112,

		RTA_KillFattyIntradayUntilItDies_74939, //remove april 2019
		RTA_ReviewHistoricalAdherence_Domain_74770,
		RTA_SpeedUpHistoricalAdherence_RemoveLastBefore_78306,
		RTA_SpeedUpHistoricalAdherence_EventStoreUpgrader_78485,
		RTA_SpeedUpHistoricalAdherence_RemoveScheduleDependency_78485,
		RTA_TooManyPersonAssociationChangedEvents_Packages_78669,
		RTA_StateQueueFloodPrevention_77710,
		RTA_ReviewHistoricalAdherence_74770,

		MyTimeWeb_NewTeamScheduleView_75989,
		MyTimeWeb_NewTeamScheduleViewDesktop_76313,
		MyTimeWeb_TrafficLightOnMobileDayView_77447,
		MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640,

		Wfm_MinimumScaffolding_32659,
		Wfm_WebPlan_Pilot_46815,
		Wfm_FetchBusinessHierarchyFromReadModel_45275,
		
		WfmTeamSchedule_IncreaseLimitionTo750ForScheduleQuery_74871,
		WfmTeamSchedule_SuggestShiftCategory_152,
		WfmTeamSchedule_DisplaySchedulesInShiftEditor_75978,

		Wfm_Requests_Approve_Based_On_Budget_Allotment_39626,
		Wfm_Requests_Approve_Based_On_Intraday_39868,
		Wfm_Requests_Approve_Based_On_Minimum_Approval_Time_40274,

		Wfm_People_ImportAndCreateAgentFromFile_42528,
		Wfm_People_SetFallbackValuesForImporting_43289,
		Wfm_People_MoveImportJobToBackgroundService_43582,

		//Bockemon
		Wfm_PeopleWeb_PrepareForRelease_74903,
		Wfm_Authentication_ChangePasswordMenu_76666,
		Wfm_User_Password_Reset_74957,
		Wfm_ReadNotificationConfigurationFromDb_78242,

		WfmGlobalLayout_personalOptions_37114,

		WFM_Gamification_Permission_76546,

		SchedulePeriod_HideChineseMonth_78424,
		ResourcePlanner_HideSkillPrioSliders_41312,
		ResourcePlanner_RunPerfTestAsTeam_43537,
		ResourcePlanner_UseErlangAWithInfinitePatience_45845,
		ResourcePlanner_PrepareToRemoveExportSchedule_46576,
		ResourcePlanner_UseErlangAWithInfinitePatienceEsl_74899,
		ResourcePlanner_UseErlangAWithFinitePatience_47738,
		ResourcePlanner_RespectClosedDaysWhenDoingDOBackToLegal_76348,
		ResourcePlanner_DoNotRemoveShiftsDayOffOptimization_77941,
		ResourcePlanner_HintShiftBagCannotFulFillContractTime_78717,
		ResourcePlanner_LoadLessPersonAccountsWhenOpeningScheduler_78487,
		ResourcePlanner_ConsiderOpenHoursWhenDecidingPossibleWorkTimes_76118,

		Wfm_DisplayOnlineHelp_39402,


		Wfm_SkillPriorityRoutingGUI_39885,
		ETL_Show_Tenant_Name_In_History_75767,

		WfmStaffing_AddOvertime_42524,
		OvertimeRequestUseMostUnderStaffedSkill_47853,
		OvertimeRequestChangeBelongsToDateForOverNightShift_74984,
		WFM_Forecaster_Refact_44480,
		WFM_Export_Forecast_44716,
		WFM_Forecaster_Preview_74801,
		WFM_Forecast_Show_History_Data_76432,
		WFM_TrainingPlanner_44780,
		Report_Remove_Realtime_Scheduled_Time_Per_Activity_45560,
		Report_Remove_Realtime_Scheduled_Time_vs_Target_45559,

		WFM_AuditTrail_44006,
		Report_Show_Utc_In_Report_Selection_When_In_Use_45079,
		WFM_Gamification_Setting_With_External_Quality_Values_45003,
		WFM_Gamification_Calculate_Badges_47250,
		WFM_Gamification_Recalculate_Badges_Within_Period_48403,
		WFM_Gamification_Create_Rolling_Periods_74866,
		Report_Remove_Realtime_AuditTrail_44006,
		ETL_Show_Web_Based_ETL_Tool_74837,
		WFM_Clear_Data_After_Leaving_Date_47768,
		WFM_Request_View_Permissions_77731,
		WFM_Request_Show_Feedback_Link_77733,
		WFM_Request_Show_Shift_for_Absence_Requests_79008,

		MyTimeWeb_ShiftTradeRequest_MaximumWorkdayCheck_74889,
		MyTimeWeb_ShiftTradeRequest_ShowMultipleShifts_74947,
		MyTimeWeb_ShiftTradeRequest_SetNotSelectableShifts_77052,
		MyTimeWeb_ShiftTradeRequest_SelectShiftsForTrade_76306,
		MyTimeWeb_ShiftTradeRequest_BalanceToleranceTime_77408,
		MyTimeWeb_ShiftTradeRequest_BackShiftTradeView_77409,
		MyTimeWeb_Request_CleanUpRequestHisotry_77776,
		MyTimeWeb_AbsenceRequest_LimitAbsenceTypes_77446,

		WFM_Intraday_Redesign_77214,

		WFM_ChatBot_77547,
		WFM_Insights_78059,

		Wfm_Payroll_SupportMultiDllPayrolls_75959,
		Wfm_Stardust_EnableScaleout_77366,
		Wfm_AuditTrail_StaffingAuditTrail_78125,
		Wfm_AuditTrail_GenericAuditTrail_74938,
		WFM_Connect_NewLandingPage_Remove_GDPR_78132,
		WFM_Connect_NewLandingPage_WEB_78578,
		WFM_AbsenceRequest_ImproveThroughput_79139
		// ReSharper restore InconsistentNaming
	}
}
