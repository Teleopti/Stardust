namespace Teleopti.Ccc.Domain.FeatureFlags
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
		Request_ShiftTradeRequestForMoreDays_20918,
		Request_SeePossibleShiftTradesFromAllTeams_28770,
		Request_FilterPossibleShiftTradeByTime_24560,
		Request_RecalculatePersonAccountBalanceOnRequestConsumer_36850,
		Settings_SetAgentDescription_23257,
		Settings_AlertViaEmailFromSMSLink_30444,

		RTA_AdherenceDetails_34267,
		RTA_CalculatePercentageInAgentTimezone_31236,
		RTA_NeutralAdherence_30930,
		RTA_DeletedPersons_36041,
		RTA_TerminatedPersons_36042,
		RTA_TeamChanges_36043,

		MessageBroker_SchedulingScreenMailbox_32733,

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

		ResourcePlanner_WeeklyRestSolver_35043,

		Wfm_Requests_Basic_35986,
		Wfm_Requests_People_Search_36294,
		Wfm_Requests_Performance_36295,
		Wfm_Requests_ApproveDeny_36297,
		Wfm_Requests_Waitlist_36232,

		Report_UseOpenXmlFormat_35797,
		Scheduler_IntradayOptimization_36617,
		Scheduler_AgentSkillAnalyzer_12345,
		Wfm_Use_Stardust,
		Wfm_ForecastFileImportOnStardust_37047,

		ResourcePlanner_SkillGroupDeleteAfterCalculation_37048,
		ResourcePlanner_JumpOutWhenLargeGroupIsHalfOptimized_37049,
        ETL_SpeedUpPersonPeriodIntraday_37162
        // ReSharper restore InconsistentNaming
    }
}