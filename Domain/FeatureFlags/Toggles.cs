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
		
		Request_RecalculatePersonAccountBalanceOnRequestConsumer_36850,

		RTA_KillFattyIntradayUntilItDies_74939,
		RTA_RestrictModifyAdherenceWithPermission_74898,
		RTA_EasilySpotLateForWork_75668,
		LevelUp_HangfireStatistics_76139_76373,
		
		MyTimeWeb_SortRequestList_40711,
		MyTimeWeb_PreferenceForJalaliCalendar_42965,
		MyTimeWeb_DayScheduleForStartPage_43446,
		MyTimeWeb_DayScheduleForStartPage_Command_44209,
		MyTimeWeb_MonthlyScheduleMobileView_45004,
		MyTimeWeb_WaitListPositionEnhancement_46301,
		MyTimeWeb_PollToCheckScheduleChanges_46595,
		MyTimeWeb_NewTeamScheduleView_75989,

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
		Wfm_Requests_DenyRequestWhenAllSkillsClosed_46384,
		AddOrRemoveTenantsWithoutRestart_43635,

		Wfm_People_ImportAndCreateAgentFromFile_42528,
		Wfm_People_SetFallbackValuesForImporting_43289,
		Wfm_People_MoveImportJobToBackgroundService_43582,
		Wfm_PeopleWeb_PrepareForRelease_47766,

		Scheduler_RestrictionReport_47013,
		Scheduler_RestrictionReportToggleAll_76224,

		People_SpeedUpOpening_76365,

		WfmGlobalLayout_personalOptions_37114,
		
		ResourcePlanner_HideSkillPrioSliders_41312,
		ResourcePlanner_RunPerfTestAsTeam_43537,
		ResourcePlanner_UseErlangAWithInfinitePatience_45845,
		ResourcePlanner_DayOffOptimizationIslands_47208,
		ResourcePlanner_PrepareToRemoveExportSchedule_46576,
		ResourcePlanner_SpeedUpEvents_48769,
		ResourcePlanner_SpeedUpEvents_74996,
		ResourcePlanner_SpeedUpEvents_75415,
		ResourcePlanner_UseErlangAWithInfinitePatienceEsl_74899,
		ResourcePlanner_UseErlangAWithFinitePatience_47738,
		ResourcePlanner_LessResourcesXXL_74915,

		Wfm_DisplayOnlineHelp_39402,

		ETL_SpeedUpFactScheduleNightly_38019,
		ETL_SpeedUpPersonPeriodNightly_38097,
		ETL_SpeedUpNightlyReloadDatamart_38131,
		ETL_SpeedUpNightlySkill_37543,
		ETL_SpeedUpScenarioNightly_38300,
		ETL_SpeedUpNightlyActivity_38303,
		ETL_SpeedUpNightlyOvertime_38304,
		ETL_SpeedUpNightlyAbsence_38301,
		ETL_MoveBadgeCalculationToETL_38421,
		ETL_SpeedUpNightlyShiftCategory_38718,
		ETL_SpeedUpNightlyPreference_38283,
		ETL_SpeedUpNightlyAvailability_38926,
		ETL_SpeedUpNightlyRequest_38914,
		ETL_SpeedUpGroupPagePersonNightly_37623,
		ETL_SpeedUpNightlyWorkload_38928,
		ETL_SpeedUpNightlyForecastWorkload_38929,
		ETL_UseBpoResources_75855,

		ETL_SpeedUpIntradayBusinessUnit_38932,
		ETL_SpeedUpNightlyBusinessUnit_38932,

		ETL_EventbasedDate_39562,
		NewPasswordHash_40460,
		Wfm_SkillPriorityRoutingGUI_39885,
		ETL_EventbasedTimeZone_40870,
		ETL_RemoveTimeZoneAndDateNightly_40870,
		ETL_EventBasedAgentNameDescription_41432,
		ETL_FixScheduleForPersonPeriod_41393,

		Staffing_Info_Configuration_44687,
		WfmStaffing_AddOvertime_42524,
		OvertimeRequestUseMostUnderStaffedSkill_47853,
		OvertimeRequestAtLeastOneCriticalUnderStaffedSkill_74944,
		OvertimeRequestSupportMultiSelectionSkillTypes_74945,
		OvertimeRequestStaffingCheckMethod_74949,
		OvertimeRequestChangeBelongsToDateForOverNightShift_74984,
		OvertimeRequestUsePrimarySkillOption_75573,
		ETL_SpeedUpNightlyDayOff_38213,
		MyTimeWeb_MobileResponsive_43826,
		WFM_Forecaster_Refact_44480,
		WFM_Export_Forecast_44716,
		Staffing_BPO_Visualization_74958,
		WFM_TrainingPlanner_44780,
		Report_Remove_Realtime_Scheduled_Time_Per_Activity_45560,
		Report_Remove_Realtime_Scheduled_Time_vs_Target_45559,
		Report_Remove_Scheduled_Time_Per_Activity_From_Scheduler_45640,
		
		WFM_WebScheduling_LowPriority_44320,
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

		People_ImprovePersonAccountAccuracy_74914,
		MyTimeWeb_ShiftTradeRequest_MaximumWorkdayCheck_74889,
		MyTimeWeb_ShiftTradeRequest_ShowMultipleShifts_74947,

		WFM_Intraday_Refactoring_74652,

		Tech_Moving_ResilientConnectionLogic_76181,
		Tenant_PurgeLogonAttempts_75782,
		Tenant_PurgeNoneEmployeesData_76291,
		Staffing_BPO_ClearStaffing_75498,
		Staffing_BPO_ExportMultipleSkills_74968
		// ReSharper restore InconsistentNaming
	}
}
