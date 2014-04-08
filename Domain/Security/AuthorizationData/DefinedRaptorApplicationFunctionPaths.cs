namespace Teleopti.Ccc.Domain.Security.AuthorizationData
{
	/// <summary>
	/// Pre defined application function constants.
	/// </summary>
	public static class DefinedRaptorApplicationFunctionPaths
	{
		// level 0
		public const string All = "All";
		public const string OpenRaptorApplication = "Raptor";

		// level 1
		public const string RaptorGlobal = "Raptor/Global";
		public const string OpenSchedulePage = "Raptor/Scheduler";
		public const string OpenForecasterPage = "Raptor/Forecaster";
		public const string OpenPersonAdminPage = "Raptor/PersonAdmin";
		public const string AccessToReports = "Raptor/Reports";
		public const string AccessToOnlineReports = "Raptor/OnlineReports";
		public const string OpenPermissionPage = "Raptor/Permission";
		public const string Shifts = "Raptor/Shifts";
		public const string OpenOptionsPage = "Raptor/Options";
		public const string OpenIntradayPage = "Raptor/Intraday";
		public const string OpenBudgets = "Raptor/Budgets";
		public const string AccessToPerformanceManager = "Raptor/PerformanceManager";
		public const string PayrollIntegration = "Raptor/PayrollIntegration";
		public const string UnderConstruction = "Raptor/UnderConstruction";
		public const string OpenAgentPortal = "Raptor/AgentPortal";
		public const string MyTimeWeb = "Raptor/MyTimeWeb";
		public const string MobileReports = "Raptor/MobileReports";
		public const string Anywhere = "Raptor/Anywhere";


		// Global
		public const string ModifyPersonAssignment = "Raptor/Global/ModifySchedule/ModifyAssignment";
		public const string ModifyPersonAbsence = "Raptor/Global/ModifySchedule/ModifyAbsence";
		public const string ModifyPersonRestriction = "Raptor/Global/ModifySchedule/ModifyPersonRestriction";
		public const string ViewSchedules = "Raptor/Global/ViewSchedules";
		public const string ViewUnpublishedSchedules = "Raptor/Global/ViewUnpublishedSchedules";
		public const string ModifyMeetings = "Raptor/Global/ModifyMeetings";
		public const string ModifyWriteProtectedSchedule = "Raptor/Global/ModifyWriteProtectedSchedule";
		public const string SetWriteProtection = "Raptor/Global/SetWriteProtection";
		public const string ViewConfidential = "Raptor/Global/ViewConfidential";
		public const string ModifySchedule = "Raptor/Global/ModifySchedule";
		public const string ViewRestrictedScenario = "Raptor/Global/ViewRestrictedScenario";
		public const string ModifyRestrictedScenario = "Raptor/Global/ModifyRestrictedScenario";
		public const string ViewActiveAgents = "Raptor/Global/ViewActiveAgents";
		public const string ModifyAvailabilities = "Raptor/Global/ModifyAvailabilities";
		public const string SignInAsAnotherUser = "Raptor/Global/SignInAsAnotherUser";

		// PersonAdmin
		public const string ModifyPersonNameAndPassword = "Raptor/PersonAdmin/ModifyPersonNameAndPassword";
		public const string ModifyGroupPage = "Raptor/PersonAdmin/ModifyGroupPage";
		public const string ModifyPeopleWithinGroupPage = "Raptor/PersonAdmin/ModifyPeopleWithinGroupPage";
		public const string SendAsm = "Raptor/PersonAdmin/SendAsm";
		public const string AllowPersonModifications = "Raptor/PersonAdmin/AllowPersonModifications";

		// Options
		public const string ManageRealTimeAdherence = "Raptor/Options/RTA";
		public const string ManageScorecards = "Raptor/Options/Scorecards";
		public const string AbsenceRequests = "Raptor/Options/AbsenceRequests";
		public const string ShiftTradeRequests = "Raptor/Options/ShiftTradeRequests";
		public const string AuditTrailSettings = "Raptor/Options/AuditTrailSettings";

		// Scheduler
		public const string AutomaticScheduling = "Raptor/Scheduler/AutomaticScheduling";
		public const string RequestScheduler = "Raptor/Scheduler/Request";
		public const string RequestSchedulerApprove = "Raptor/Scheduler/Request/Approve";

		// Forecaster
		public const string ExportForecastToOtherBusinessUnit = "Raptor/Forecaster/ExportForecastToOtherBusinessUnit";
		public const string ImportForecastFromFile = "Raptor/Forecaster/ImportForecastFromFile";
		public const string ExportForecastFile = "Raptor/Forecaster/ExportForecastFile";

		// Budget
		public const string RequestAllowances = "Raptor/Budgets/RequestAllowances";

		// Agent Portal
		public const string OpenAsm = "Raptor/AgentPortal/ASM";
		public const string ModifyShiftCategoryPreferences = "Raptor/AgentPortal/ShiftCategoryPreferences";
		public const string ModifyExtendedPreferences = "Raptor/AgentPortal/ExtendedPreferences";
		public const string OpenMyReport = "Raptor/AgentPortal/MyReport";
		public const string CreateTextRequest = "Raptor/AgentPortal/TextRequests";
		public const string CreateShiftTradeRequest = "Raptor/AgentPortal/ShiftTradeRequests";
		public const string CreateAbsenceRequest = "Raptor/AgentPortal/AbsenceRequests";
		public const string OpenScorecard = "Raptor/AgentPortal/Scorecard";
		public const string CreateStudentAvailability = "Raptor/AgentPortal/StudentAvailability";
		public const string ViewSchedulePeriodCalculation = "Raptor/AgentPortal/ViewSchedulePeriodCalculation";
		public const string SetPlanningTimeBank = "Raptor/AgentPortal/SetPlanningTimeBank";
		public const string ViewCustomTeamSchedule = "Raptor/AgentPortal/ViewCustomTeamSchedule";

		// Intraday
		public const string IntradayRealTimeAdherence = "Raptor/Intraday/RTA";
		public const string IntradayEarlyWarning = "Raptor/Intraday/EW";
		public const string IntradayReForecasting = "Raptor/Intraday/ReForecasting";

		// Performance Manager
		public const string CreatePerformanceManagerReport = "Raptor/PerformanceManager/CreatePerformanceManagerReport";
		public const string ViewPerformanceManagerReport = "Raptor/PerformanceManager/ViewPerformanceManagerReport";

		// Online Reports
		public const string ScheduledTimePerActivityReport = "Raptor/OnlineReports/ScheduledTimePerActivityReport";
		public const string ScheduleAuditTrailReport = "Raptor/OnlineReports/ScheduleAuditTrailReport";
		public const string ScheduleTimeVersusTargetTimeReport = "Raptor/OnlineReports/ScheduleTimeVersusTargetTimeReport";

		// Agent Portal Web
		public const string StudentAvailability = "Raptor/MyTimeWeb/StudentAvailability";
		public const string StandardPreferences = "Raptor/MyTimeWeb/StandardPreferences";
		public const string TextRequests = "Raptor/MyTimeWeb/TextRequests";
		public const string TeamSchedule = "Raptor/MyTimeWeb/TeamSchedule";
		public const string ViewAllGroupPages = "Raptor/MyTimeWeb/TeamSchedule/ViewAllGroupPages";
		public const string AbsenceRequestsWeb = "Raptor/MyTimeWeb/AbsenceRequests";
		public const string AgentScheduleMessenger = "Raptor/MyTimeWeb/AgentScheduleMessenger";
		public const string ExtendedPreferencesWeb = "Raptor/MyTimeWeb/ExtendedPreferences";
		public const string ShiftTradeRequestsWeb = "Raptor/MyTimeWeb/ShiftTradeRequests";
		public const string ShareCalendar = "Raptor/MyTimeWeb/ShareCalendar";
		public const string OvertimeAvailabilityWeb = "Raptor/MyTimeWeb/OvertimeAvailabilityWeb";
		public const string MyReportWeb = "Raptor/MyTimeWeb/MyReportWeb";
		public const string ViewPersonalAccount = "Raptor/MyTimeWeb/ViewPersonalAccount";

		// Anywhere
		public const string SchedulesAnywhere = "Raptor/Anywhere/Schedules";
		public const string RealTimeAdherenceOverview = "Raptor/Anywhere/RealTimeAdherenceOverview";
	}
}
