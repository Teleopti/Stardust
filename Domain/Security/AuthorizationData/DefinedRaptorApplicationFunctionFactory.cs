using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.AuthorizationData
{
	public interface IDefinedRaptorApplicationFunctionFactory
	{
		/// <summary>
		/// Gets or sets the logged on user authorization service instance.
		/// </summary>
		/// <value>The instance.</value>
		/// <remarks>
		/// Do not use this method in tests. Singeltons are unreliable in tests. Use the CreateApplicationFunctionList() method instead.
		/// </remarks>
		IEnumerable<IApplicationFunction> ApplicationFunctionList { get; }
	}

	/// <summary>
	/// Pre-defined Raptor application functions list.
	/// </summary>
	public class DefinedRaptorApplicationFunctionFactory : IDefinedRaptorApplicationFunctionFactory
	{
		private readonly Lazy<ReadOnlyCollection<IApplicationFunction>> _definedApplicationFunctions = new Lazy<ReadOnlyCollection<IApplicationFunction>>(createApplicationFunctionList);

		/// <summary>
		/// Gets or sets the logged on user authorization service instance.
		/// </summary>
		/// <value>The instance.</value>
		/// <remarks>
		/// Do not use this method in tests. Singeltons are unreliable in tests. Use the CreateApplicationFunctionList() method instead.
		/// </remarks>
		public IEnumerable<IApplicationFunction> ApplicationFunctionList
		{
			get
			{
				return _definedApplicationFunctions.Value;
			}
		}

		private static ReadOnlyCollection<IApplicationFunction> createApplicationFunctionList()
		{
			List<IApplicationFunction> applicationFunctionList = new List<IApplicationFunction>();

			// level 0 root 
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.All, "xxAll", DefinedRaptorApplicationFunctionForeignIds.All, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.OpenRaptorApplication, "xxOpenRaptorApplication", DefinedRaptorApplicationFunctionForeignIds.OpenRaptorApplication, 10);

			//level 1 modules 
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.RaptorGlobal, "xxGlobalFunctions", DefinedRaptorApplicationFunctionForeignIds.RaptorGlobal, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.OpenSchedulePage, "xxOpenSchedulePage", DefinedRaptorApplicationFunctionForeignIds.OpenSchedulePage, 50);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.OpenForecasterPage, "xxForecasts", DefinedRaptorApplicationFunctionForeignIds.OpenForecasterPage, 30);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage, "xxOpenPersonAdminPage", DefinedRaptorApplicationFunctionForeignIds.OpenPersonAdminPage, 20);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.AccessToReports, "xxReports", DefinedRaptorApplicationFunctionForeignIds.AccessToReports, 60);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.AccessToOnlineReports, "xxOnlineReports", DefinedRaptorApplicationFunctionForeignIds.AccessToOnlineReports, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.OpenPermissionPage, "xxOpenPermissionPage", DefinedRaptorApplicationFunctionForeignIds.OpenPermissionPage, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.Shifts, "xxShifts", DefinedRaptorApplicationFunctionForeignIds.Shifts, 40);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.OpenOptionsPage, "xxOptions", DefinedRaptorApplicationFunctionForeignIds.OpenOptionsPage, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.OpenIntradayPage, "xxIntraday", DefinedRaptorApplicationFunctionForeignIds.OpenIntradayPage, 55);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.OpenBudgets, "xxBudgets", DefinedRaptorApplicationFunctionForeignIds.OpenBudgets, 70);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.AccessToPerformanceManager, "xxPerformanceManager", DefinedRaptorApplicationFunctionForeignIds.AccessToPerformanceManager, 80);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.PayrollIntegration, "xxPayrollIntegration", DefinedRaptorApplicationFunctionForeignIds.PayrollIntegration, 200);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.SeatPlanner, "xxSeatPlanner", DefinedRaptorApplicationFunctionForeignIds.SeatPlanner, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.Outbound, "xxOutbound", DefinedRaptorApplicationFunctionForeignIds.Outbound, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.AngelMyTeamSchedules, "xxMyTeam", DefinedRaptorApplicationFunctionForeignIds.AngelMyTeamSchedules, null);

			// Global
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ModifySchedule, "xxModifySchedule", DefinedRaptorApplicationFunctionForeignIds.ModifySchedule, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment, "xxModifyAssignment", DefinedRaptorApplicationFunctionForeignIds.ModifyPersonAssignment, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence, "xxModifyAbsence", DefinedRaptorApplicationFunctionForeignIds.ModifyPersonAbsence, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ModifyPersonRestriction, "xxModifyPersonRestriction", DefinedRaptorApplicationFunctionForeignIds.ModifyPersonRestriction, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ViewSchedules, "xxViewSchedules", DefinedRaptorApplicationFunctionForeignIds.ViewSchedules, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules, "xxViewUnpublishedSchedules", DefinedRaptorApplicationFunctionForeignIds.ViewUnpublishedSchedules, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ModifyMeetings, "xxModifyMeetings", DefinedRaptorApplicationFunctionForeignIds.ModifyMeetings, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule, "xxModifyWriteProtectedSchedule", DefinedRaptorApplicationFunctionForeignIds.ModifyWriteProtectedSchedule, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.SetWriteProtection, "xxSetWriteProtection", DefinedRaptorApplicationFunctionForeignIds.SetWriteProtection, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ViewConfidential, "xxViewConfidential", DefinedRaptorApplicationFunctionForeignIds.ViewConfidential, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ViewRestrictedScenario, "xxViewRestrictedScenario", DefinedRaptorApplicationFunctionForeignIds.ViewRestrictedScenario, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ModifyRestrictedScenario, "xxModifyRestrictedScenario", DefinedRaptorApplicationFunctionForeignIds.ModifyRestrictedScenario, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ViewActiveAgents, "xxViewActiveAgents", DefinedRaptorApplicationFunctionForeignIds.ViewActiveAgents, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ModifyAvailabilities, "xxModifyAvailabilities", DefinedRaptorApplicationFunctionForeignIds.ModifyAvailabilities, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.PublishSchedule, "xxPublishSchedule", DefinedRaptorApplicationFunctionForeignIds.PublishSchedule, null);

			// PersonAdmin
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ModifyPersonNameAndPassword, "xxModifyPersonNameAndPassword", DefinedRaptorApplicationFunctionForeignIds.ModifyPersonNameAndPassword, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ModifyGroupPage, "xxModifyGroupPage", DefinedRaptorApplicationFunctionForeignIds.ModifyGroupPage, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ModifyPeopleWithinGroupPage, "xxModifyPeopleWithinGroupPage", DefinedRaptorApplicationFunctionForeignIds.ModifyPeopleWithinGroupPage, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.SendAsm, "xxSendAsm", DefinedRaptorApplicationFunctionForeignIds.SendAsm, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.AllowPersonModifications, "xxAllowPersonModifications", DefinedRaptorApplicationFunctionForeignIds.AllowPersonModifications, null);

			// Options
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ManageRealTimeAdherence, "xxManageRTA", DefinedRaptorApplicationFunctionForeignIds.ManageRealTimeAdherence, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ManageScorecards, "xxManageScorecards", DefinedRaptorApplicationFunctionForeignIds.ManageScorecards, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.AbsenceRequests, "xxAbsenceRequest", DefinedRaptorApplicationFunctionForeignIds.AbsenceRequests, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequests, "xxShiftTradeRequest", DefinedRaptorApplicationFunctionForeignIds.ShiftTradeRequests, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.AuditTrailSettings, "xxAuditTrailSettings", DefinedRaptorApplicationFunctionForeignIds.AuditTrailSettings, null);

			// Scheduler
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.AutomaticScheduling, "xxAutomaticScheduling", DefinedRaptorApplicationFunctionForeignIds.AutomaticScheduling, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.RequestScheduler, "xxRequests", DefinedRaptorApplicationFunctionForeignIds.RequestScheduler, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.RequestSchedulerApprove, "xxApprove", DefinedRaptorApplicationFunctionForeignIds.RequestSchedulerApprove, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.RequestSchedulerViewAllowance, "xxViewAllowance", DefinedRaptorApplicationFunctionForeignIds.RequestSchedulerViewAllowance, null);

			// Forecaster
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ExportForecastToOtherBusinessUnit, "xxExportForecastToOtherBusinessUnit", DefinedRaptorApplicationFunctionForeignIds.ExportForecastToOtherBusinessUnit, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ImportForecastFromFile, "xxImportForecastFromFile", DefinedRaptorApplicationFunctionForeignIds.ImportForecastFromFile, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ExportForecastFile, "xxExportToFile", DefinedRaptorApplicationFunctionForeignIds.ExportForecastFile, null);

			// Budget
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.RequestAllowances, "xxRequestAllowances", DefinedRaptorApplicationFunctionForeignIds.RequestAllowances, null);

			// Intraday
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.IntradayRealTimeAdherence, "xxRealTimeAdherence", DefinedRaptorApplicationFunctionForeignIds.IntradayRealTimeAdherence, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.IntradayEarlyWarning, "xxEarlyWarning", DefinedRaptorApplicationFunctionForeignIds.IntradayEarlyWarning, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.IntradayReForecasting, "xxReforecast", DefinedRaptorApplicationFunctionForeignIds.IntradayReForecasting, null);

			// Performance Manager
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.CreatePerformanceManagerReport, "xxCreatePerformanceManagerReport", DefinedRaptorApplicationFunctionForeignIds.CreatePerformanceManagerReport, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ViewPerformanceManagerReport, "xxViewPerformanceManagerReport", DefinedRaptorApplicationFunctionForeignIds.ViewPerformanceManagerReport, null);

			// Online Reports
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ScheduledTimePerActivityReport, "xxScheduledTimePerActivityReport", DefinedRaptorApplicationFunctionForeignIds.ScheduledTimePerActivityReport, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ScheduleAuditTrailReport, "xxScheduleAuditTrailReport", DefinedRaptorApplicationFunctionForeignIds.ScheduleAuditTrailReport, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ScheduleTimeVersusTargetTimeReport, "xxScheduledTimeVsTarget", DefinedRaptorApplicationFunctionForeignIds.ScheduleTimeVersusTargetTimeReport, null);

			// Agent Portal Web
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.MyTimeWeb, "xxMyTimeWeb", DefinedRaptorApplicationFunctionForeignIds.MyTimeWeb, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.StudentAvailability, "xxStudentAvailability", DefinedRaptorApplicationFunctionForeignIds.StudentAvailability, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.StandardPreferences, "xxModifyShiftCategoryPreferences", DefinedRaptorApplicationFunctionForeignIds.StandardPreferences, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.TextRequests, "xxCreateTextRequest", DefinedRaptorApplicationFunctionForeignIds.TextRequests, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.TeamSchedule, "xxTeamSchedule", DefinedRaptorApplicationFunctionForeignIds.TeamSchedule, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ViewAllGroupPages, "xxViewAllGroupPages", DefinedRaptorApplicationFunctionForeignIds.ViewAllGroupPages, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb, "xxAbsenceRequestsWeb", DefinedRaptorApplicationFunctionForeignIds.AbsenceRequestsWeb, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ExtendedPreferencesWeb, "xxExtendedPreferencesWeb", DefinedRaptorApplicationFunctionForeignIds.ExtendedPreferencesWeb, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger, "xxAgentScheduleMessengerPermission", DefinedRaptorApplicationFunctionForeignIds.AgentScheduleMessenger, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb, "xxShiftTradeRequests", DefinedRaptorApplicationFunctionForeignIds.ShiftTradeRequestsWeb, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ShiftTradeBulletinBoard, "xxShiftTradeBulletinBoard", DefinedRaptorApplicationFunctionForeignIds.ShiftTradeBulletinBoard, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ShareCalendar, "xxShareCalendar", DefinedRaptorApplicationFunctionForeignIds.ShareCalendar, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.OvertimeAvailabilityWeb, "xxOvertimeAvailabilityWeb", DefinedRaptorApplicationFunctionForeignIds.OvertimeAvailabilityWeb, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.MyReportWeb, "xxMyReportWeb", DefinedRaptorApplicationFunctionForeignIds.MyReportWeb, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ViewPersonalAccount, "xxViewPersonalAccount", DefinedRaptorApplicationFunctionForeignIds.ViewPersonalAccount, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.MyReportQueueMetrics, "xxQueueMetrics", DefinedRaptorApplicationFunctionForeignIds.MyReportQueueMetrics, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ViewBadge, "xxViewBadge", DefinedRaptorApplicationFunctionForeignIds.ViewBadge, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, "xxViewBadgeLeaderboard", DefinedRaptorApplicationFunctionForeignIds.ViewBadgeLeaderboard, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.AbsenceReport, "xxAbsenceReport", DefinedRaptorApplicationFunctionForeignIds.AbsenceReport, null);	

			// Web
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.Anywhere, "xxAnywhere", DefinedRaptorApplicationFunctionForeignIds.Anywhere, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, "xxMyTeamSchedules", DefinedRaptorApplicationFunctionForeignIds.MyTeamSchedules, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.AddFullDayAbsence, "xxAddFullDayAbsence", DefinedRaptorApplicationFunctionForeignIds.AddFullDayAbsence, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.AddIntradayAbsence, "xxAddIntradayAbsence", DefinedRaptorApplicationFunctionForeignIds.AddIntradayAbsence, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.RemoveAbsence, "xxDeleteAbsence", DefinedRaptorApplicationFunctionForeignIds.RemoveAbsence, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.AddActivity, "xxAddActivity", DefinedRaptorApplicationFunctionForeignIds.AddActivity, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.MoveActivity, "xxMoveActivity", DefinedRaptorApplicationFunctionForeignIds.MoveActivity, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.SwapShifts, "xxSwapShifts", DefinedRaptorApplicationFunctionForeignIds.SwapShifts, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, "xxRealTimeAdherenceOverview", DefinedRaptorApplicationFunctionForeignIds.RealTimeAdherenceOverview, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.WebForecasts, "xxForecasts", DefinedRaptorApplicationFunctionForeignIds.WebForecasts, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.WebPermissions, "xxPermissions", DefinedRaptorApplicationFunctionForeignIds.WebPermissions, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.WebSchedules, "xxSchedules", DefinedRaptorApplicationFunctionForeignIds.WebSchedules, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.WebPeople, "xxPeople", DefinedRaptorApplicationFunctionForeignIds.WebPeople, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.WebRequests, "xxRequests", DefinedRaptorApplicationFunctionForeignIds.WebRequests, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.WebModifySkill, "xxModifySkill", DefinedRaptorApplicationFunctionForeignIds.WebModifySkill, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.WebIntraday, "xxIntraday", DefinedRaptorApplicationFunctionForeignIds.WebIntraday, null);
			createAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.WebModifySkillArea, "xxModifySkillArea", DefinedRaptorApplicationFunctionForeignIds.WebModifySkillArea, null);

			return new ReadOnlyCollection<IApplicationFunction>(applicationFunctionList);
		}

		/// <summary>
		/// Creates a new application function.
		/// </summary>
		/// <param name="applicationFunctionList">The application function list.</param>
		/// <param name="functionPath">The function path.</param>
		/// <param name="functionDescription">The function description.</param>
		/// <param name="definedKey">The foreign GUID id.</param>
		/// <param name="sortOrder"></param>
		private static void createAndAddApplicationFunction(ICollection<IApplicationFunction> applicationFunctionList, string functionPath, string functionDescription, string definedKey, int? sortOrder)
		{
			string codeName = ApplicationFunction.GetCode(functionPath);
			string parentPath = ApplicationFunction.GetParentPath(functionPath);

			var newFunction = new ApplicationFunction
			{
				FunctionCode = codeName,
				FunctionDescription = functionDescription,
				Parent = ApplicationFunction.FindByPath(applicationFunctionList, parentPath),
				ForeignId = definedKey,
				ForeignSource = DefinedForeignSourceNames.SourceRaptor,
				SortOrder = sortOrder
			};
			applicationFunctionList.Add(newFunction);
		}
	}
}
