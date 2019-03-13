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
		RTA_ReviewHistoricalAdherence_74770,
		RTA_RestrictModifySkillGroups_78568,
		RTA_InputValidationForApprovingAdherencePeriods_77045,
		RTA_ImproveUsabilityExpandableCards_79025,
		RTA_AdjustAdherenceToNeutral_80594,
		RTA_RemoveAdjustedToNeutral_82147,
		
		MessageBroker_HttpSenderThrottleRequests_79140,
		MessageBroker_VeganBurger_79140,
		MessageBroker_ServerThrottleMessages_79140,
		MessageBroker_ScheduleChangedMessagePackaging_79140,
		
		MyTimeWeb_NewTeamScheduleView_75989,
		MyTimeWeb_NewTeamScheduleViewDesktop_76313,
		MyTimeWeb_TrafficLightOnMobileDayView_77447,
		MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640,
		MyTimeWeb_NewAbsenceRequestWaitlistPosition_80131,

		Wfm_MinimumScaffolding_32659,
		Wfm_WebPlan_Pilot_46815,
		
		WfmTeamSchedule_SuggestShiftCategory_152,
		WfmTeamSchedule_ShiftEditorInDayView_78295,
		WfmTeamSchedule_DisableAutoRefreshSchedule_79826,
		WfmTeamSchedule_OnlyRefreshScheduleForRelevantChangesInWeekView_80242,
		WfmTeamSchedule_VirtualRepeatInScheduleTable_80404,

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
		Wfm_AutomaticNotificationEnrollment_79679,
		Wfm_AutomaticNotificationEnrollmentStartup_79679,
		Wfm_PeopleWeb_Improve_Search_81681,

		WfmGlobalLayout_personalOptions_37114,

		SchedulePeriod_HideChineseMonth_78424,
		ResourcePlanner_HideSkillPrioSliders_41312,
		ResourcePlanner_HideExportSchedule_81161,
		ResourcePlanner_HintShiftBagCannotFulFillContractTime_78717,
		ResourcePlanner_LoadLessPersonAccountsWhenOpeningScheduler_78487,
		ResourcePlanner_ConsiderOpenHoursWhenDecidingPossibleWorkTimes_76118,
		ResourcePlanner_QueryHintOnLayers_79780,
		ResourcePlanner_TeamSchedulingInPlans_79283,
		ResourcePlanner_DelayShiftCreations_81680,
		ResourcePlanner_PrepareToRemoveRightToLeft_81112,
		WFM_Plans_Redesign_81198,
		WFM_Plans_HeatMap_79112_80785,
		WFM_Plans_IntradayIssuesInHeatMap_79113,

		Wfm_DisplayOnlineHelp_39402,

		Status_ShowAdmin_82154,

		Wfm_SkillPriorityRoutingGUI_39885,
		WfmStaffing_AddOvertime_42524,
		OvertimeRequestChangeBelongsToDateForOverNightShift_74984,
		WFM_Forecaster_Preview_74801,
		WFM_Forecast_Show_History_Data_76432,
		WFM_TrainingPlanner_44780,
		Report_Remove_Realtime_Scheduled_Time_vs_Target_45559,

		WFM_Clear_Data_After_Leaving_Date_47768,
		WFM_Setting_BankHolidayCalendar_Create_79297,
		WFM_Setting_AssignBankHolidayCalendarsToSites_79899,
		
		MyTimeWeb_Preference_Indicate_BankHoliday_79900,
		MyTimeWeb_Schedule_MobileMonth_Indicate_BankHoliday_79901,
		MyTimeWeb_Schedule_WeekView_Indicate_BankHoliday_81154,
		MyTimeWeb_Schedule_MonthView_Indicate_BankHoliday_81158,
		MyTimeWeb_Availability_Indicate_BankHoliday_81656,

		WFM_Intraday_Redesign_77214,
		WFM_Intraday_Browser_Tab_81482,

		WFM_ChatBot_77547,
		WFM_Insights_80704,

		Wfm_Payroll_SupportMultiDllPayrolls_75959,
		Wfm_AuditTrail_StaffingAuditTrail_78125,
		Wfm_AuditTrail_GenericAuditTrail_74938,
		WFM_Connect_NewLandingPage_Remove_GDPR_78132,
		WFM_Connect_NewLandingPage_WEB_78578,
		WFM_AbsenceRequest_ImproveThroughput_79139,
		WFM_ProbabilityView_ImproveResponseTime_80040,
		WFM_Intraday_OptimizeSkillDayLoad_80153,
		WFM_Log_Analytics_Schedule_Change_Hangfire_handler_80425,
		ETL_Intraday_SpeedUp_Fact_Schedule_Deviation_Calculation_79646,
		WFM_AbsenceRequest_Robust_Processing_79988,
		WFM_AnalyticsForecastUpdater_80798,
		WFM_Forecast_Readmodel_80790,
		WFM_PurgeUsersWithinDays_77460
		// ReSharper restore InconsistentNaming
	}
}