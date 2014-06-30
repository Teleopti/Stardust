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
		private ReadOnlyCollection<IApplicationFunction> _definedApplicationFunctions;
		private static readonly object LockObject = new object();

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
				lock (LockObject)
				{
					if (_definedApplicationFunctions == null)
					{
						_definedApplicationFunctions = CreateApplicationFunctionList();
					}
				}
				return _definedApplicationFunctions;
			}
		}

		private static ReadOnlyCollection<IApplicationFunction> CreateApplicationFunctionList()
		{
			List<IApplicationFunction> applicationFunctionList = new List<IApplicationFunction>();

			// level 0 root 
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.All, "xxAll", DefinedRaptorApplicationFunctionForeignIds.All, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.OpenRaptorApplication, "xxOpenRaptorApplication", DefinedRaptorApplicationFunctionForeignIds.OpenRaptorApplication, 10);

			//level 1 modules 
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.RaptorGlobal, "xxGlobalFunctions", DefinedRaptorApplicationFunctionForeignIds.RaptorGlobal, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.OpenSchedulePage, "xxOpenSchedulePage", DefinedRaptorApplicationFunctionForeignIds.OpenSchedulePage, 50);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.OpenForecasterPage, "xxForecasts", DefinedRaptorApplicationFunctionForeignIds.OpenForecasterPage, 30);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage, "xxOpenPersonAdminPage", DefinedRaptorApplicationFunctionForeignIds.OpenPersonAdminPage, 20);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.AccessToReports, "xxReports", DefinedRaptorApplicationFunctionForeignIds.AccessToReports, 60);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.AccessToOnlineReports, "xxOnlineReports", DefinedRaptorApplicationFunctionForeignIds.AccessToOnlineReports, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.OpenPermissionPage, "xxOpenPermissionPage", DefinedRaptorApplicationFunctionForeignIds.OpenPermissionPage, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.Shifts, "xxShifts", DefinedRaptorApplicationFunctionForeignIds.Shifts, 40);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.OpenOptionsPage, "xxOptions", DefinedRaptorApplicationFunctionForeignIds.OpenOptionsPage, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.OpenIntradayPage, "xxIntraday", DefinedRaptorApplicationFunctionForeignIds.OpenIntradayPage, 55);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.OpenBudgets, "xxBudgets", DefinedRaptorApplicationFunctionForeignIds.OpenBudgets, 70);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.AccessToPerformanceManager, "xxPerformanceManager", DefinedRaptorApplicationFunctionForeignIds.AccessToPerformanceManager, 80);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.PayrollIntegration, "xxPayrollIntegration", DefinedRaptorApplicationFunctionForeignIds.PayrollIntegration, 200);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.OpenAgentPortal, "xxAgentPortal", DefinedRaptorApplicationFunctionForeignIds.OpenAgentPortal, null);

			// Global
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ModifySchedule, "xxModifySchedule", DefinedRaptorApplicationFunctionForeignIds.ModifySchedule, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment, "xxModifyAssignment", DefinedRaptorApplicationFunctionForeignIds.ModifyPersonAssignment, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence, "xxModifyAbsence", DefinedRaptorApplicationFunctionForeignIds.ModifyPersonAbsence, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ModifyPersonRestriction, "xxModifyPersonRestriction", DefinedRaptorApplicationFunctionForeignIds.ModifyPersonRestriction, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ViewSchedules, "xxViewSchedules", DefinedRaptorApplicationFunctionForeignIds.ViewSchedules, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules, "xxViewUnpublishedSchedules", DefinedRaptorApplicationFunctionForeignIds.ViewUnpublishedSchedules, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ModifyMeetings, "xxModifyMeetings", DefinedRaptorApplicationFunctionForeignIds.ModifyMeetings, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule, "xxModifyWriteProtectedSchedule", DefinedRaptorApplicationFunctionForeignIds.ModifyWriteProtectedSchedule, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.SetWriteProtection, "xxSetWriteProtection", DefinedRaptorApplicationFunctionForeignIds.SetWriteProtection, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ViewConfidential, "xxViewConfidential", DefinedRaptorApplicationFunctionForeignIds.ViewConfidential, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ViewRestrictedScenario, "xxViewRestrictedScenario", DefinedRaptorApplicationFunctionForeignIds.ViewRestrictedScenario, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ModifyRestrictedScenario, "xxModifyRestrictedScenario", DefinedRaptorApplicationFunctionForeignIds.ModifyRestrictedScenario, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ViewActiveAgents, "xxViewActiveAgents", DefinedRaptorApplicationFunctionForeignIds.ViewActiveAgents, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ModifyAvailabilities, "xxModifyAvailabilities", DefinedRaptorApplicationFunctionForeignIds.ModifyAvailabilities, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.SignInAsAnotherUser, "xxSignInAsAnotherUserInPermission", DefinedRaptorApplicationFunctionForeignIds.SignInAsAnotherUser, null);

			// PersonAdmin
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ModifyPersonNameAndPassword, "xxModifyPersonNameAndPassword", DefinedRaptorApplicationFunctionForeignIds.ModifyPersonNameAndPassword, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ModifyGroupPage, "xxModifyGroupPage", DefinedRaptorApplicationFunctionForeignIds.ModifyGroupPage, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ModifyPeopleWithinGroupPage, "xxModifyPeopleWithinGroupPage", DefinedRaptorApplicationFunctionForeignIds.ModifyPeopleWithinGroupPage, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.SendAsm, "xxSendAsm", DefinedRaptorApplicationFunctionForeignIds.SendAsm, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.AllowPersonModifications, "xxAllowPersonModifications", DefinedRaptorApplicationFunctionForeignIds.AllowPersonModifications, null);

			// Options
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ManageRealTimeAdherence, "xxManageRTA", DefinedRaptorApplicationFunctionForeignIds.ManageRealTimeAdherence, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ManageScorecards, "xxManageScorecards", DefinedRaptorApplicationFunctionForeignIds.ManageScorecards, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.AbsenceRequests, "xxAbsenceRequest", DefinedRaptorApplicationFunctionForeignIds.AbsenceRequests, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequests, "xxShiftTradeRequest", DefinedRaptorApplicationFunctionForeignIds.ShiftTradeRequests, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.AuditTrailSettings, "xxAuditTrailSettings", DefinedRaptorApplicationFunctionForeignIds.AuditTrailSettings, null);

			// Scheduler
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.AutomaticScheduling, "xxAutomaticScheduling", DefinedRaptorApplicationFunctionForeignIds.AutomaticScheduling, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.RequestScheduler, "xxRequests", DefinedRaptorApplicationFunctionForeignIds.RequestScheduler, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.RequestSchedulerApprove, "xxApprove", DefinedRaptorApplicationFunctionForeignIds.RequestSchedulerApprove, null);

			// Forecaster
			CreateAndAddApplicationFunction(applicationFunctionList,
																			DefinedRaptorApplicationFunctionPaths.ExportForecastToOtherBusinessUnit,
																			"xxExportForecastToOtherBusinessUnit",
																			DefinedRaptorApplicationFunctionForeignIds.ExportForecastToOtherBusinessUnit, null);
			CreateAndAddApplicationFunction(applicationFunctionList,
																DefinedRaptorApplicationFunctionPaths.ImportForecastFromFile,
																			"xxImportForecastFromFile",
								DefinedRaptorApplicationFunctionForeignIds.ImportForecastFromFile, null);
			CreateAndAddApplicationFunction(applicationFunctionList,
											DefinedRaptorApplicationFunctionPaths.ExportForecastFile,
											"xxExportToFile",
											DefinedRaptorApplicationFunctionForeignIds.ExportForecastFile, null);
			// Budget
			CreateAndAddApplicationFunction(applicationFunctionList,
																			DefinedRaptorApplicationFunctionPaths.RequestAllowances,
																						"xxRequestAllowances",
																						DefinedRaptorApplicationFunctionForeignIds.RequestAllowances, null);
			// Agent Portal
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.OpenAsm, "xxASM", DefinedRaptorApplicationFunctionForeignIds.OpenAsm, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ModifyShiftCategoryPreferences, "xxModifyShiftCategoryPreferences", DefinedRaptorApplicationFunctionForeignIds.ModifyShiftCategoryPreferences, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ModifyExtendedPreferences, "xxExtendedPreferences", DefinedRaptorApplicationFunctionForeignIds.ModifyExtendedPreferences, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.OpenMyReport, "xxOpenMyReport", DefinedRaptorApplicationFunctionForeignIds.OpenMyReport, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.CreateTextRequest, "xxCreateTextRequest", DefinedRaptorApplicationFunctionForeignIds.CreateTextRequest, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.CreateShiftTradeRequest, "xxCreateShiftTradeRequest", DefinedRaptorApplicationFunctionForeignIds.CreateShiftTradeRequest, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.CreateAbsenceRequest, "xxCreateAbsenceRequest", DefinedRaptorApplicationFunctionForeignIds.CreateAbsenceRequest, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.OpenScorecard, "xxOpenScorecard", DefinedRaptorApplicationFunctionForeignIds.OpenScorecard, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.CreateStudentAvailability, "xxCreateStudentAvailability", DefinedRaptorApplicationFunctionForeignIds.CreateStudentAvailability, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ViewSchedulePeriodCalculation, "xxViewSchedulePeriodCalculation", DefinedRaptorApplicationFunctionForeignIds.ViewSchedulePeriodCalculation, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.SetPlanningTimeBank, "xxSetPlanningTimeBank", DefinedRaptorApplicationFunctionForeignIds.SetPlanningTimeBank, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ViewCustomTeamSchedule, "xxViewCustomTeamSchedule", DefinedRaptorApplicationFunctionForeignIds.ViewCustomTeamSchedule, null);

			// Intraday
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.IntradayRealTimeAdherence, "xxRealTimeAdherence", DefinedRaptorApplicationFunctionForeignIds.IntradayRealTimeAdherence, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.IntradayEarlyWarning, "xxEarlyWarning", DefinedRaptorApplicationFunctionForeignIds.IntradayEarlyWarning, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.IntradayReForecasting, "xxReforecast", DefinedRaptorApplicationFunctionForeignIds.IntradayReForecasting, null);

			// Performance Manager
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.CreatePerformanceManagerReport, "xxCreatePerformanceManagerReport", DefinedRaptorApplicationFunctionForeignIds.CreatePerformanceManagerReport, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ViewPerformanceManagerReport, "xxViewPerformanceManagerReport", DefinedRaptorApplicationFunctionForeignIds.ViewPerformanceManagerReport, null);

			// Online Reports
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ScheduledTimePerActivityReport, "xxScheduledTimePerActivityReport", DefinedRaptorApplicationFunctionForeignIds.ScheduledTimePerActivityReport, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ScheduleAuditTrailReport, "xxScheduleAuditTrailReport", DefinedRaptorApplicationFunctionForeignIds.ScheduleAuditTrailReport, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ScheduleTimeVersusTargetTimeReport, "xxScheduledTimeVsTarget", DefinedRaptorApplicationFunctionForeignIds.ScheduleTimeVersusTargetTimeReport, null);

			// Agent Portal Web
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.MyTimeWeb, "xxMyTimeWeb", DefinedRaptorApplicationFunctionForeignIds.MyTimeWeb, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.StudentAvailability, "xxStudentAvailability", DefinedRaptorApplicationFunctionForeignIds.StudentAvailability, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.StandardPreferences, "xxModifyShiftCategoryPreferences", DefinedRaptorApplicationFunctionForeignIds.StandardPreferences, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.TextRequests, "xxCreateTextRequest", DefinedRaptorApplicationFunctionForeignIds.TextRequests, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.TeamSchedule, "xxTeamSchedule", DefinedRaptorApplicationFunctionForeignIds.TeamSchedule, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ViewAllGroupPages, "xxViewAllGroupPages", DefinedRaptorApplicationFunctionForeignIds.ViewAllGroupPages, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb, "xxAbsenceRequestsWeb", DefinedRaptorApplicationFunctionForeignIds.AbsenceRequestsWeb, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ExtendedPreferencesWeb, "xxExtendedPreferencesWeb", DefinedRaptorApplicationFunctionForeignIds.ExtendedPreferencesWeb, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger, "xxAgentScheduleMessengerPermission", DefinedRaptorApplicationFunctionForeignIds.AgentScheduleMessenger, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb, "xxShiftTradeRequests", DefinedRaptorApplicationFunctionForeignIds.ShiftTradeRequestsWeb, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ShareCalendar, "xxShareCalendar", DefinedRaptorApplicationFunctionForeignIds.ShareCalendar, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.OvertimeAvailabilityWeb, "xxOvertimeAvailabilityWeb", DefinedRaptorApplicationFunctionForeignIds.OvertimeAvailabilityWeb, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.MyReportWeb, "xxMyReportWeb", DefinedRaptorApplicationFunctionForeignIds.MyReportWeb, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.ViewPersonalAccount, "xxViewPersonalAccount", DefinedRaptorApplicationFunctionForeignIds.ViewPersonalAccount, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.MyReportQueueMetrics, "xxQueueMetrics", DefinedRaptorApplicationFunctionForeignIds.MyReportQueueMetrics, null);

			// Anywhere
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.Anywhere, "xxAnywhere", DefinedRaptorApplicationFunctionForeignIds.Anywhere, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, "xxSchedules", DefinedRaptorApplicationFunctionForeignIds.MyTeamSchedules, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.AddFullDayAbsence, "xxAddFullDayAbsence", DefinedRaptorApplicationFunctionForeignIds.AddFullDayAbsence, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.AddIntradayAbsence, "xxAddIntradayAbsence", DefinedRaptorApplicationFunctionForeignIds.AddIntradayAbsence, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.RemoveAbsence, "xxRemoveAbsence", DefinedRaptorApplicationFunctionForeignIds.RemoveAbsence, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.AddActivity, "xxAddActivity", DefinedRaptorApplicationFunctionForeignIds.AddActivity, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.MoveActivity, "xxMoveActivity", DefinedRaptorApplicationFunctionForeignIds.MoveActivity, null);
			CreateAndAddApplicationFunction(applicationFunctionList, DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, "xxRealTimeAdherenceOverview", DefinedRaptorApplicationFunctionForeignIds.RealTimeAdherenceOverview, null);

			return new ReadOnlyCollection<IApplicationFunction>(applicationFunctionList);
		}

		/// <summary>
		/// Creates a new application function.
		/// </summary>
		/// <param name="applicationFunctionList">The application function list.</param>
		/// <param name="functionPath">The function path.</param>
		/// <param name="functionDescription">The function description.</param>
		/// <param name="definedKey">The foreign GUID id.</param>
		private static IApplicationFunction CreateAndAddApplicationFunction(ICollection<IApplicationFunction> applicationFunctionList, string functionPath, string functionDescription, string definedKey, int? sortOrder)
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
			return newFunction;
		}
	}
}
