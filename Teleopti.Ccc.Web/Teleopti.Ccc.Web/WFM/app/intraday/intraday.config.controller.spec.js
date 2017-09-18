'use strict';
describe('IntradayConfigController', function () {
	var $httpBackend,
		$controller,
		$translate,
		scope,
		vm;

	var skills = [];
	var toggles = [];

	beforeEach(module('wfm.intraday'));

	beforeEach(function () {
		skills = [
			{
				Id: "fa9b5393-ef48-40d1-b7cc-09e797589f81",
				Name: "my skill"
			}];
		
		toggles = {
			"TestToggle": false,
			"TestToggle2": false,
			"TestToggle3": false,
			"Forecast_CopySettingsToWorkflow_11112": false,
			"Preference_PreferenceAlertWhenMinOrMaxHoursBroken_25635": true,
			"Request_RecalculatePersonAccountBalanceOnRequestConsumer_36850": true,
			"Settings_SetAgentDescription_23257": true,
			"Settings_AlertViaEmailFromSMSLink_30444": true,
			"RTA_SpreadTransactionLocksStrategy_41823": true,
			"RTA_EventPackagesOptimization_43924": true,
			"RTA_AsyncOptimization_43924": true,
			"RTA_RememberMyPartOfTheBusiness_39082": true,
			"MyTimeWeb_TeamScheduleNoReadModel_36210": true,
			"MyTimeWeb_ShiftTradePossibleTradedSchedulesNoReadModel_36211": true,
			"MyTimeWeb_ShiftTradeBoardNoReadModel_36662": true,
			"MyTimeWeb_MakeRequestsResponsiveOnMobile_40266": true,
			"MyTimeWeb_ShiftTradeFilterSite_40374": true,
			"MyTimeWeb_ShiftTradeOptimization_40792": true,
			"MyTimeWeb_PreferenceForMobile_40265": true,
			"MyTimeWeb_StudentAvailabilityForMobile_42498": true,
			"MyTimeWeb_ValidateAbsenceRequestsSynchronously_40747": true,
			"MyTimeWeb_ShowContractTime_41462": true,
			"MyTimeWeb_SortRequestList_40711": true,
			"MyTimeWeb_ViewIntradayStaffingProbability_41608": true,
			"MyTimeWeb_ViewIntradayStaffingProbabilityOnMobile_42913": true,
			"MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880": true,
			"MyTimeWeb_PreferenceForJalaliCalendar_42965": true,
			"MyTimeWeb_PreferencePerformanceForMultipleUsers_43322": true,
			"MyTimeWeb_DayScheduleForStartPage_43446": true,
			"MyTimeWeb_DayScheduleForStartPage_Command_44209": true,
			"MyTimeWeb_ViewWFMAppGuide_43848": true,
			"MyTimeWeb_CalculateOvertimeProbabilityByPrimarySkill_44686": true,
			"MyTimeWeb_OvertimeRequest_44558": true,
			"Backlog_Module_23980": false,
			"Gamification_NewBadgeCalculation_31185": true,
			"MyTimeWeb_ShowSeatBookingMonthView_39068": true,
			"Wfm_MinimumScaffolding_32659": true,
			"Wfm_ResourcePlanner_32892": true,
			"Wfm_ChangePlanningPeriod_33043": true,
			"WfmIntraday_MonitorActualvsForecasted_35176": true,
			"Wfm_HideUnusedTeamsAndSites_42690": true,
			"Wfm_SearchAgentBasedOnCorrectPeriod_44552": true,
			"Wfm_AddMyTimeLink_45088": true,
			"Wfm_GroupPages_45057": true,
			"Wfm_FetchBusinessHierarchyFromReadModel_45275": true,
			"WfmTeamSchedule_MoveOvertimeActivity_44888": true,
			"WfmTeamSchedule_SortRows_45056": true,
			"WfmTeamSchedule_DisplayAndEditPublicNote_44783": true,
			"WebByPassDefaultPermissionCheck_37984": true,
			"WfmReportPortal_Basic_38825": true,
			"WfmReportPortal_LeaderBoard_39440": true,
			"Wfm_Requests_Approve_Based_On_Budget_Allotment_39626": true,
			"Wfm_Requests_Approve_Based_On_Intraday_39868": true,
			"Wfm_Requests_Approve_Based_On_Minimum_Approval_Time_40274": true,
			"Wfm_Requests_Configurable_BusinessRules_For_ShiftTrade_40770": true,
			"Wfm_Requests_TriggerResourceCalculationFromGui_43129": false,
			"Wfm_Requests_OvertimeRequestHandling_45177": true,
			"Wfm_Requests_Refactoring_45470": true,
			"Wfm_Requests_ProcessWaitlistBefore24hRequests_45767": false,
			"Wfm_Outbound_IgnoreScheduleForReplan_43752": true,
			"Wfm_Seatplan_UseDatePeriodForPlanning_42167": true,
			"Wfm_Intraday_38074": true,
			"Wfm_People_PrepareForRelease_39040": true,
			"Wfm_People_ImportAndCreateAgentFromFile_42528": true,
			"Wfm_People_SetFallbackValuesForImporting_43289": true,
			"Wfm_People_MoveImportJobToBackgroundService_43582": true,
			"Report_UseOpenXmlFormat_35797": true,
			"Scheduler_IntradayOptimization_36617": true,
			"Scheduler_ShowSkillPrioLevels_41980": true,
			"WfmGlobalLayout_personalOptions_37114": true,
			"ResourcePlanner_CascadingSkillsGUI_40018": false,
			"ResourcePlanner_HideSkillPrioSliders_41312": true,
			"ResourcePlanner_RunPerfTestAsTeam_43537": false,
			"ResourcePlanner_MergeTeamblockClassicScheduling_44289": true,
			"ResourcePlanner_MergeTeamblockClassicIntraday_45508": true,
			"ResourcePlanner_RespectSkillGroupShoveling_44156": true,
			"ResourcePlanner_RetireKeepPercentageOfShifts_45688": true,
			"ResourcePlanner_SpeedUpShiftsWithinDay_45694": false,
			"Wfm_DisplayOnlineHelp_39402": true,
			"ETL_SpeedUpFactScheduleNightly_38019": true,
			"ETL_SpeedUpPersonPeriodNightly_38097": true,
			"ETL_SpeedUpNightlyReloadDatamart_38131": true,
			"ETL_SpeedUpNightlySkill_37543": true,
			"ETL_SpeedUpScenarioNightly_38300": true,
			"Scheduler_LoadWithoutRequests_38567": true,
			"ETL_SpeedUpNightlyActivity_38303": true,
			"ETL_SpeedUpNightlyOvertime_38304": true,
			"ETL_SpeedUpNightlyAbsence_38301": true,
			"ETL_MoveBadgeCalculationToETL_38421": true,
			"ETL_SpeedUpNightlyShiftCategory_38718": true,
			"ETL_SpeedUpNightlyPreference_38283": true,
			"ETL_FasterIndexMaintenance_38847": true,
			"ETL_SpeedUpNightlyAvailability_38926": true,
			"ETL_SpeedUpNightlyRequest_38914": true,
			"ETL_SpeedUpGroupPagePersonNightly_37623": true,
			"ETL_SpeedUpNightlyWorkload_38928": true,
			"ETL_SpeedUpNightlyForecastWorkload_38929": true,
			"HealthCheck_ValidateReadModelPersonScheduleDay_39421": true,
			"HealthCheck_ValidateReadModelScheduleDay_39423": true,
			"HealthCheck_EasyValidateAndFixReadModels_39696": true,
			"HealthCheck_ReinitializeReadModels_39697": true,
			"ETL_SpeedUpIntradayBusinessUnit_38932": true,
			"ETL_SpeedUpNightlyBusinessUnit_38932": true,
			"QRCodeForMobileApps_42695": true,
			"ConfigQRCodeURLForMobileApps_43224": true,
			"MobileApps_EnableMobileNotifications_44476": true,
			"ETL_EventbasedDate_39562": true,
			"NewPasswordHash_40460": true,
			"Wfm_Intraday_OptimalStaffing_40921": true,
			"Wfm_SkillPriorityRoutingGUI_39885": true,
			"ETL_EventbasedTimeZone_40870": true,
			"ETL_RemoveTimeZoneAndDateNightly_40870": true,
			"ETL_EventBasedAgentNameDescription_41432": true,
			"ETL_FixScheduleForPersonPeriod_41393": true,
			"Wfm_ArchiveSchedule_41498": true,
			"Wfm_ImportSchedule_41247": true,
			"SchedulingScreen_BatchAgentValidation_41552": true,
			"Wfm_Intraday_ScheduledStaffing_41476": true,
			"Mailbox_Optimization_41900": true,
			"No_UnitOfWork_Nesting_42175": true,
			"Wfm_Intraday_ESL_41827": true,
			"Wfm_Web_Intraday_Rta_As_first_Choice_42206": true,
			"Landing_Page_Data_Protection_Question_35721": true,
			"Wfm_ResourcePlanner_SchedulingOnStardust_42874": true,
			"Staffing_Info_Configuration_44687": true,
			"WfmStaffing_AddOvertime_42524": true,
			"Reporting_Optional_Columns_42066": true,
			"Staffing_ReadModel_Keep8DaysHistoricalData_44652": true,
			"ETL_SpeedUpNightlyDayOff_38213": true,
			"Wfm_Intraday_SupportSkillTypeWebChat_42591": true,
			"MyTimeWeb_MobileResponsive_43826": true,
			"Staffing_ReadModel_BetterAccuracy_Step3_44331": true,
			"Staffing_ReadModel_BetterAccuracy_Step4_43389": true,
			"Wfm_Intraday_SupportSkillTypeEmail_44002": true,
			"WFM_Intraday_Show_For_Other_Days_43504": true,
			"WFM_Intraday_SupportMultisiteSkill_43874": true,
			"WFM_Intraday_Export_To_Excel_44892": true,
			"WFM_Forecaster_Refact_44480": true,
			"WFM_Export_Forecast_44716": true,
			"WFM_RedirectPermissionToWeb_44562": true,
			"Staffing_BPOExchangeImport_45202": true,
			"WFM_TrainingPlanner_44780": true,
			"Report_Remove_Realtime_Scheduled_Time_Per_Activity_45560": true,
			"Report_Remove_Realtime_Scheduled_Time_vs_Target_45559": true,
			"Report_Remove_Scheduled_Time_Per_Activity_From_Scheduler_45640": true,
			"Wfm_Staffing_45411": true,
			"WFM_WebScheduling_LowPriority_44320": true,
			"WFM_Intraday_SupportOtherSkillsLikeEmail_44026": true,
			"WFM_Unified_Skill_Group_Management_45417": false,
			"WFM_AuditTrail_44006": false
		};
	});

	beforeEach(inject(function (_$httpBackend_, _$controller_, _$rootScope_, _$translate_) {
		$controller = _$controller_;
		$httpBackend = _$httpBackend_;
		scope = _$rootScope_.$new();
		$translate = _$translate_;

		$httpBackend.whenGET("../api/intraday/skills")
			.respond(200, skills);
		
		$httpBackend.whenGET("../ToggleHandler/AllToggles").respond(200, toggles);
	}));

	var createController = function (isNewlyCreatedSkillArea) {
		vm = $controller('IntradayConfigController', {
			$scope: scope,
			$translate: $translate
		});
		scope.$digest();
		$httpBackend.flush();
	};

	it('should display list of skills', function () {
		createController();
		expect(vm.skills[0].Id).toEqual("fa9b5393-ef48-40d1-b7cc-09e797589f81");
		expect(vm.skills[0].Name).toEqual("my skill");
	});
});
