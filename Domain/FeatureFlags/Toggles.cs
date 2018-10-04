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

		Request_RecalculatePersonAccountBalanceOnRequestConsumer_36850,

		RTA_KillFattyIntradayUntilItDies_74939,
		RTA_RestrictModifyAdherenceWithPermission_74898,
		RTA_EasilySpotLateForWork_75668,
		LevelUp_HangfireStatistics_76139_76373,
		RTA_DurationOfHistoricalEvents_76470,
		RTA_ReviewHistoricalAdherence_74770,

		MyTimeWeb_SortRequestList_40711,
		MyTimeWeb_PreferenceForJalaliCalendar_42965,
		MyTimeWeb_DayScheduleForStartPage_43446,
		MyTimeWeb_DayScheduleForStartPage_Command_44209,
		MyTimeWeb_MonthlyScheduleMobileView_45004,
		MyTimeWeb_WaitListPositionEnhancement_46301,
		MyTimeWeb_PollToCheckScheduleChanges_46595,
		MyTimeWeb_NewTeamScheduleView_75989,
		MyTimeWeb_NewTeamScheduleViewDesktop_76313,

		Wfm_MinimumScaffolding_32659,
		Wfm_WebPlan_Pilot_46815,
		WfmIntraday_MonitorActualvsForecasted_35176,
		Wfm_HideUnusedTeamsAndSites_42690,
		Wfm_SearchAgentBasedOnCorrectPeriod_44552,
		Wfm_AddMyTimeLink_45088,
		Wfm_GroupPages_45057,
		Wfm_FetchBusinessHierarchyFromReadModel_45275,

		WfmTeamSchedule_ShowStaffingInfo_45206,
		WfmTeamSchedule_ExportSchedulesToExcel_45795,
		WfmTeamSchedule_ShowSkillsForSelectedSkillGroupInStaffingInfo_47202,
		WfmTeamSchedule_AddAbsenceFromPartOfDayToXDay_46010,
		WfmTeamSchedule_AddNDeleteDayOff_40555,
		WfmTeamSchedule_RemoveShift_46322,
		WfmTeamSchedule_IncreaseLimitionTo750ForScheduleQuery_74871,
		WfmTeamSchedule_SuggestShiftCategory_152,
		WfmTeamSchedule_ShowInformationForUnderlyingSchedule_74952,
		WfmTeamSchedule_DisplaySchedulesInShiftEditor_75978,

		WebByPassDefaultPermissionCheck_37984,

		Wfm_Requests_Approve_Based_On_Budget_Allotment_39626,
		Wfm_Requests_Approve_Based_On_Intraday_39868,
		Wfm_Requests_Approve_Based_On_Minimum_Approval_Time_40274,

		Wfm_People_ImportAndCreateAgentFromFile_42528,
		Wfm_People_SetFallbackValuesForImporting_43289,
		Wfm_People_MoveImportJobToBackgroundService_43582,

		//Bockemon
		Wfm_PeopleWeb_PrepareForRelease_74903,
		Wfm_Authentication_ChangePasswordMenu_76666,

		People_SpeedUpOpening_76365,

		WfmGlobalLayout_personalOptions_37114,

		WFM_Gamification_Permission_76546,

		ResourcePlanner_HideSkillPrioSliders_41312,
		ResourcePlanner_RunPerfTestAsTeam_43537,
		ResourcePlanner_UseErlangAWithInfinitePatience_45845,
		ResourcePlanner_PrepareToRemoveExportSchedule_46576,
		ResourcePlanner_UseErlangAWithInfinitePatienceEsl_74899,
		ResourcePlanner_UseErlangAWithFinitePatience_47738,
		ResourcePlanner_RespectClosedDaysWhenDoingDOBackToLegal_76348,
		ResourcePlanner_NoWhiteSpotWhenTargetDayoffIsBroken_77941,
		ResourcePlanner_BetterFitPreferences_76289,
		
		Wfm_ArchiveScheduleForPast_77958,

		Wfm_DisplayOnlineHelp_39402,


		Wfm_SkillPriorityRoutingGUI_39885,
		ETL_Optimize_Memory_Usage_76761,
		ETL_Show_Tenant_Name_In_History_75767,

		Staffing_Info_Configuration_44687,
		WfmStaffing_AddOvertime_42524,
		OvertimeRequestUseMostUnderStaffedSkill_47853,
		OvertimeRequestAtLeastOneCriticalUnderStaffedSkill_74944,
		OvertimeRequestSupportMultiSelectionSkillTypes_74945,
		OvertimeRequestStaffingCheckMethod_74949,
		OvertimeRequestChangeBelongsToDateForOverNightShift_74984,
		OvertimeRequestUsePrimarySkillOption_75573,
		MyTimeWeb_MobileResponsive_43826,
		WFM_Forecaster_Refact_44480,
		WFM_Export_Forecast_44716,
		WFM_Forecaster_Preview_74801,
		WFM_Forecast_Show_History_Data_76432,
		WFM_TrainingPlanner_44780,
		Report_Remove_Realtime_Scheduled_Time_Per_Activity_45560,
		Report_Remove_Realtime_Scheduled_Time_vs_Target_45559,
		Report_Remove_Scheduled_Time_Per_Activity_From_Scheduler_45640,

		WFM_Modify_Skill_Groups_43727,
		WFM_AuditTrail_44006,
		Report_Show_Utc_In_Report_Selection_When_In_Use_45079,
		WFM_Gamification_Setting_With_External_Quality_Values_45003,
		WFM_Gamification_Calculate_Badges_47250,
		WFM_Gamification_Recalculate_Badges_Within_Period_48403,
		WFM_Gamification_Create_Rolling_Periods_74866,
		Report_Remove_Realtime_AuditTrail_44006,
		ETL_Show_Web_Based_ETL_Tool_74837,
		WFM_Remember_My_Selection_In_Intraday_47254,
		WFM_Clear_Data_After_Leaving_Date_47768,
		
		MyTimeWeb_ShiftTradeRequest_MaximumWorkdayCheck_74889,
		MyTimeWeb_ShiftTradeRequest_ShowMultipleShifts_74947,
		MyTimeWeb_ShiftTradeRequest_SetNotSelectableShifts_77052,
		MyTimeWeb_ShiftTradeRequest_SelectShiftsForTrade_76306,
		MyTimeWeb_ShiftTradeRequest_BalanceToleranceTime_77408,
		MyTimeWeb_ShiftTradeRequest_BackShiftTradeView_77409,
		MyTimeWeb_Request_CleanUpRequestHisotry_77776,
		MyTimeWeb_AbsenceRequest_LimitAbsenceTypes_77446,

		WFM_Intraday_Refactoring_74652,
		WFM_Intraday_Redesign_77214,

		Tech_Moving_ResilientConnectionLogic_76181,
		WFM_ChatBot_77547,

		Wfm_Payroll_SupportMultiDllPayrolls_75959,
		Wfm_Stardust_EnableScaleout_77366
		// ReSharper restore InconsistentNaming
	}
}
