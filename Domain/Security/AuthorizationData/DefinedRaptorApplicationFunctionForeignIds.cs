namespace Teleopti.Ccc.Domain.Security.AuthorizationData
{
	/// <summary>
	/// Pre-defined application function foreign id constants.
	/// </summary>
	/// <remarks>
	/// Structure of a key: Unique Id
	/// Must be unique. Do NOT USE the Unique Id even of a deleted function to avoid conflict. 
	/// Use a higher number than the current maximum Unique Id which is 0092 currently.  
	/// </remarks>
	public static class DefinedRaptorApplicationFunctionForeignIds
	{
		// level 0
		public const string All = "0000";
		public const string OpenRaptorApplication = "0001";

		// level 1
		public const string RaptorGlobal = "0023";
		public const string OpenSchedulePage = "0002";
		public const string OpenForecasterPage = "0003";
		public const string OpenPersonAdminPage = "0004";
		public const string AccessToReports = "0006";
		public const string AccessToOnlineReports = "0054";
		public const string OpenPermissionPage = "0008";
		public const string Shifts = "0016";
		public const string OpenOptionsPage = "0017";
		public const string OpenIntradayPage = "0018";
		public const string OpenBudgets = "0050";
		public const string AccessToPerformanceManager = "0040";
		public const string PayrollIntegration = "0044";
		public const string UnderConstruction = "0048";
		public const string OpenAgentPortal = "0019";

		// Global
		public const string ModifyPersonAssignment = "0014";
		public const string ModifyPersonAbsence = "0012";
		public const string ModifyPersonDayOff = "0013";
		public const string ModifyPersonRestriction = "0039";
		public const string ViewSchedules = "0049";
		public const string ViewUnpublishedSchedules = "0025";
		public const string ModifyMeetings = "0043";
		public const string ModifyWriteProtectedSchedule = "0045";
		public const string SetWriteProtection = "0046";
		public const string ViewConfidential = "0052";
		public const string ModifySchedule = "0057";
		public const string ViewRestrictedScenario = "0061";
		public const string ModifyRestrictedScenario = "0062";
		public const string ViewActiveAgents = "0086";
		public const string ModifyAvailabilities = "0087";
		public const string SignInAsAnotherUser = "0094";

		// PersonAdmin
		public const string ModifyPersonNameAndPassword = "0007";
		public const string ModifyGroupPage = "0037";
		public const string ModifyPeopleWithinGroupPage = "0038";
		public const string SendAsm = "0067";
		public const string AllowPersonModifications = "0071";

		// Options
		public const string ManageRealTimeAdherence = "0032";
		public const string ManageScorecards = "0033";
		public const string AbsenceRequests = "0053";
		public const string ShiftTradeRequests = "0056";
		public const string AuditTrailSettings = "0058";

		// Scheduler
		public const string AutomaticScheduling = "0009";
		public const string RequestScheduler = "0021";
		public const string RequestSchedulerApprove = "0022";

		// Forecaster
		public const string ExportForecastToOtherBusinessUnit = "0073";
		public const string ImportForecastFromFile = "0076";
		public const string ExportForecastFile = "0082";

		// Budget
		public const string RequestAllowances = "0075";

		// Agent Portal
		public const string OpenAsm = "0020";
		public const string ModifyShiftCategoryPreferences = "0026";
		public const string OpenMyReport = "0027";
		public const string CreateTextRequest = "0028";
		public const string CreateShiftTradeRequest = "0029";
		public const string CreateAbsenceRequest = "0030";
		public const string OpenScorecard = "0031";
		public const string CreateStudentAvailability = "0036";
		public const string ModifyExtendedPreferences = "0051";
		public const string ViewSchedulePeriodCalculation = "0060";
		public const string SetPlanningTimeBank = "0063";
		public const string ViewCustomTeamSchedule = "0069";

		// Intraday
		public const string IntradayRealTimeAdherence = "0034";
		public const string IntradayEarlyWarning = "0035";
		public const string IntradayReForecasting = "0085";

		// Performance Manager
		public const string CreatePerformanceManagerReport = "0041";
		public const string ViewPerformanceManagerReport = "0042";

		// Online reports
		public const string ScheduledTimePerActivityReport = "0055";
		public const string ScheduleAuditTrailReport = "0059";
		public const string ScheduleTimeVersusTargetTimeReport = "0064";

		// Agent Portal Web
		public const string MyTimeWeb = "0065";
		public const string StudentAvailability = "0066";
		public const string StandardPreferences = "0068";
		public const string TextRequests = "0070";
		public const string TeamSchedule = "0072";
		public const string AbsenceRequestsWeb = "0077";
		public const string AgentScheduleMessenger = "0078";
		public const string ExtendedPreferencesWeb = "0079";
		public const string ShiftTradeRequestsWeb = "0083";
		public const string ViewAllGroupPages = "0084";
		public const string ShareCalendar = "0088";
		public const string OvertimeAvailabilityWeb = "0089";
		public const string MyReportWeb = "0090";
		public const string ViewPersonalAccount = "0093";
		public const string MyReportQueueMetrics = "0095";

		// Mobile Reports
		public const string MobileReports = "0074";

		// Anywhere
		public const string Anywhere = "0080";
		public const string SchedulesAnywhere = "0081";
		public const string RealTimeAdherenceOverview = "0092";
	}
}
