using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.Domain.Security.AuthorizationData
{
	public interface IDefinedRaptorApplicationFunctionFactory
	{
		IApplicationFunction[] ApplicationFunctions { get; }
	}

	/// <summary>
	/// Pre-defined Raptor application functions list.
	/// </summary>
	public class DefinedRaptorApplicationFunctionFactory : IDefinedRaptorApplicationFunctionFactory
	{
		private readonly Lazy<IApplicationFunction[]> _definedApplicationFunctions = new Lazy<IApplicationFunction[]>(createApplicationFunctionList);

		/// <summary>
		/// Gets or sets the logged on user authorization service instance.
		/// </summary>
		/// <value>The instance.</value>
		/// <remarks>
		/// Do not use this method in tests. Singeltons are unreliable in tests. Use the CreateApplicationFunctionList() method instead.
		/// </remarks>
		public IApplicationFunction[] ApplicationFunctions => _definedApplicationFunctions.Value;

		private static IApplicationFunction[] createApplicationFunctionList()
		{
			var result = new List<IApplicationFunction>();

			// level 0 root 
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.All, "xxAll", DefinedRaptorApplicationFunctionForeignIds.All);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.OpenRaptorApplication, "xxOpenRaptorApplication", DefinedRaptorApplicationFunctionForeignIds.OpenRaptorApplication, 10);

			//level 1 modules 
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.RaptorGlobal, "xxGlobalFunctions", DefinedRaptorApplicationFunctionForeignIds.RaptorGlobal);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.OpenSchedulePage, "xxOpenSchedulePage", DefinedRaptorApplicationFunctionForeignIds.OpenSchedulePage, 50);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.OpenForecasterPage, "xxForecasts", DefinedRaptorApplicationFunctionForeignIds.OpenForecasterPage, 30);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage, "xxOpenPersonAdminPage", DefinedRaptorApplicationFunctionForeignIds.OpenPersonAdminPage, 20);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.AccessToReports, "xxReports", DefinedRaptorApplicationFunctionForeignIds.AccessToReports, 60);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.AccessToOnlineReports, "xxOnlineReports", DefinedRaptorApplicationFunctionForeignIds.AccessToOnlineReports);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.Shifts, "xxShifts", DefinedRaptorApplicationFunctionForeignIds.Shifts, 40);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.OpenOptionsPage, "xxOptions", DefinedRaptorApplicationFunctionForeignIds.OpenOptionsPage);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.OpenIntradayPage, "xxIntraday", DefinedRaptorApplicationFunctionForeignIds.OpenIntradayPage, 55);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.OpenBudgets, "xxBudgets", DefinedRaptorApplicationFunctionForeignIds.OpenBudgets, 70);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.AccessToPerformanceManager, "xxPerformanceManager", DefinedRaptorApplicationFunctionForeignIds.AccessToPerformanceManager, 80);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.PayrollIntegration, "xxPayrollIntegration", DefinedRaptorApplicationFunctionForeignIds.PayrollIntegration, 200);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.SeatPlanner, "xxSeatPlanner", DefinedRaptorApplicationFunctionForeignIds.SeatPlanner);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.Outbound, "xxOutbound", DefinedRaptorApplicationFunctionForeignIds.Outbound);

			// Global
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ModifySchedule, "xxModifySchedule", DefinedRaptorApplicationFunctionForeignIds.ModifySchedule);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment, "xxModifyAssignment", DefinedRaptorApplicationFunctionForeignIds.ModifyPersonAssignment);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence, "xxModifyAbsence", DefinedRaptorApplicationFunctionForeignIds.ModifyPersonAbsence);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ModifyPersonRestriction, "xxModifyPersonRestriction", DefinedRaptorApplicationFunctionForeignIds.ModifyPersonRestriction);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ViewSchedules, "xxViewSchedules", DefinedRaptorApplicationFunctionForeignIds.ViewSchedules);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules, "xxViewUnpublishedSchedules", DefinedRaptorApplicationFunctionForeignIds.ViewUnpublishedSchedules);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ModifyMeetings, "xxModifyMeetings", DefinedRaptorApplicationFunctionForeignIds.ModifyMeetings);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule, "xxModifyWriteProtectedSchedule", DefinedRaptorApplicationFunctionForeignIds.ModifyWriteProtectedSchedule);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.SetWriteProtection, "xxSetWriteProtection", DefinedRaptorApplicationFunctionForeignIds.SetWriteProtection);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ViewConfidential, "xxViewConfidential", DefinedRaptorApplicationFunctionForeignIds.ViewConfidential);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ViewRestrictedScenario, "xxViewRestrictedScenario", DefinedRaptorApplicationFunctionForeignIds.ViewRestrictedScenario);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ModifyRestrictedScenario, "xxModifyRestrictedScenario", DefinedRaptorApplicationFunctionForeignIds.ModifyRestrictedScenario);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ViewActiveAgents, "xxViewActiveAgents", DefinedRaptorApplicationFunctionForeignIds.ViewActiveAgents);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ModifyAvailabilities, "xxModifyAvailabilities", DefinedRaptorApplicationFunctionForeignIds.ModifyAvailabilities);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.PublishSchedule, "xxPublishSchedule", DefinedRaptorApplicationFunctionForeignIds.PublishSchedule);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.OrganizeCascadingSkills, "xxOrganizeCascadingSkills", DefinedRaptorApplicationFunctionForeignIds.OrganizeCascadingSkills);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.CopySchedule, "xxCopySchedule", DefinedRaptorApplicationFunctionForeignIds.CopySchedule);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ImportSchedule, "xxImportSchedule", DefinedRaptorApplicationFunctionForeignIds.ImportSchedule);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.SaveFavoriteSearch, "xxSaveFavoriteSearch", DefinedRaptorApplicationFunctionForeignIds.SaveFavoriteSearch);

			// PersonAdmin
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ModifyPersonNameAndPassword, "xxModifyPersonNameAndPassword", DefinedRaptorApplicationFunctionForeignIds.ModifyPersonNameAndPassword);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ModifyGroupPage, "xxModifyGroupPage", DefinedRaptorApplicationFunctionForeignIds.ModifyGroupPage);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ModifyPeopleWithinGroupPage, "xxModifyPeopleWithinGroupPage", DefinedRaptorApplicationFunctionForeignIds.ModifyPeopleWithinGroupPage);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.SendAsm, "xxSendAsm", DefinedRaptorApplicationFunctionForeignIds.SendAsm);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.AllowPersonModifications, "xxAllowPersonModifications", DefinedRaptorApplicationFunctionForeignIds.AllowPersonModifications);
            result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.DeletePerson, "xxDeletePerson", DefinedRaptorApplicationFunctionForeignIds.DeletePerson);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.AddPerson, "xxAddPerson", DefinedRaptorApplicationFunctionForeignIds.AddPerson);

			// Options
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ManageRealTimeAdherence, "xxManageRTA", DefinedRaptorApplicationFunctionForeignIds.ManageRealTimeAdherence);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ManageScorecards, "xxManageScorecards", DefinedRaptorApplicationFunctionForeignIds.ManageScorecards);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.AbsenceRequests, "xxAbsenceRequest", DefinedRaptorApplicationFunctionForeignIds.AbsenceRequests);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequests, "xxShiftTradeRequest", DefinedRaptorApplicationFunctionForeignIds.ShiftTradeRequests);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.AuditTrailSettings, "xxAuditTrailSettings", DefinedRaptorApplicationFunctionForeignIds.AuditTrailSettings);

			// Scheduler
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.AutomaticScheduling, "xxAutomaticScheduling", DefinedRaptorApplicationFunctionForeignIds.AutomaticScheduling);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.RequestScheduler, "xxRequests", DefinedRaptorApplicationFunctionForeignIds.RequestScheduler);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.RequestSchedulerApprove, "xxApprove", DefinedRaptorApplicationFunctionForeignIds.RequestSchedulerApprove);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.RequestSchedulerViewAllowance, "xxViewAllowance", DefinedRaptorApplicationFunctionForeignIds.RequestSchedulerViewAllowance);

			// Forecaster
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ExportForecastToOtherBusinessUnit, "xxExportForecastToOtherBusinessUnit", DefinedRaptorApplicationFunctionForeignIds.ExportForecastToOtherBusinessUnit);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ImportForecastFromFile, "xxImportForecastFromFile", DefinedRaptorApplicationFunctionForeignIds.ImportForecastFromFile);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ExportForecastFile, "xxExportToFile", DefinedRaptorApplicationFunctionForeignIds.ExportForecastFile);

			// Budget
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.RequestAllowances, "xxRequestAllowances", DefinedRaptorApplicationFunctionForeignIds.RequestAllowances);

			// Intraday
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.IntradayRealTimeAdherence, "xxRealTimeAdherence", DefinedRaptorApplicationFunctionForeignIds.IntradayRealTimeAdherence);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.IntradayEarlyWarning, "xxEarlyWarning", DefinedRaptorApplicationFunctionForeignIds.IntradayEarlyWarning);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.IntradayReForecasting, "xxReforecast", DefinedRaptorApplicationFunctionForeignIds.IntradayReForecasting);

			// Performance Manager
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.CreatePerformanceManagerReport, "xxCreatePerformanceManagerReport", DefinedRaptorApplicationFunctionForeignIds.CreatePerformanceManagerReport);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ViewPerformanceManagerReport, "xxViewPerformanceManagerReport", DefinedRaptorApplicationFunctionForeignIds.ViewPerformanceManagerReport);

			// Online Reports
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ScheduleTimeVersusTargetTimeReport, "xxScheduledTimeVsTarget", DefinedRaptorApplicationFunctionForeignIds.ScheduleTimeVersusTargetTimeReport);

			// Reports
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboardUnderReports, "xxBadgeLeaderBoardReport", DefinedRaptorApplicationFunctionForeignIds.ViewBadgeLeaderboardUnderReports);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ScheduleAuditTrailWebReport, "xxScheduleAuditTrailReport", DefinedRaptorApplicationFunctionForeignIds.ScheduleAuditTrailWebReport);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.GeneralAuditTrailWebReport, "xxAuditTrailGeneralAuditTrailReport", DefinedRaptorApplicationFunctionForeignIds.GeneralAuditTrailWebReport);

			// Agent Portal Web
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.MyTimeWeb, "xxMyTimeWeb", DefinedRaptorApplicationFunctionForeignIds.MyTimeWeb);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.StudentAvailability, "xxStudentAvailability", DefinedRaptorApplicationFunctionForeignIds.StudentAvailability);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.StandardPreferences, "xxModifyShiftCategoryPreferences", DefinedRaptorApplicationFunctionForeignIds.StandardPreferences);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.TextRequests, "xxCreateTextRequest", DefinedRaptorApplicationFunctionForeignIds.TextRequests);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.TeamSchedule, "xxTeamSchedule", DefinedRaptorApplicationFunctionForeignIds.TeamSchedule);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ViewAllGroupPages, "xxViewAllGroupPages", DefinedRaptorApplicationFunctionForeignIds.ViewAllGroupPages);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb, "xxAbsenceRequestsWeb", DefinedRaptorApplicationFunctionForeignIds.AbsenceRequestsWeb);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.MyTimeCancelRequest, "xxCancelRequest", DefinedRaptorApplicationFunctionForeignIds.MyTimeCancelRequest);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ExtendedPreferencesWeb, "xxExtendedPreferencesWeb", DefinedRaptorApplicationFunctionForeignIds.ExtendedPreferencesWeb);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger, "xxAgentScheduleMessengerPermission", DefinedRaptorApplicationFunctionForeignIds.AgentScheduleMessenger);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb, "xxShiftTradeRequests", DefinedRaptorApplicationFunctionForeignIds.ShiftTradeRequestsWeb);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.OvertimeRequestWeb, "xxOvertimeRequests", DefinedRaptorApplicationFunctionForeignIds.OvertimeRequestsWeb);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ShiftTradeBulletinBoard, "xxShiftTradeBulletinBoard", DefinedRaptorApplicationFunctionForeignIds.ShiftTradeBulletinBoard);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ShareCalendar, "xxShareCalendar", DefinedRaptorApplicationFunctionForeignIds.ShareCalendar);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.OvertimeAvailabilityWeb, "xxOvertimeAvailabilityWeb", DefinedRaptorApplicationFunctionForeignIds.OvertimeAvailabilityWeb);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.MyReportWeb, "xxMyReportWeb", DefinedRaptorApplicationFunctionForeignIds.MyReportWeb);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ViewPersonalAccount, "xxViewPersonalAccount", DefinedRaptorApplicationFunctionForeignIds.ViewPersonalAccount);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.MyReportQueueMetrics, "xxQueueMetrics", DefinedRaptorApplicationFunctionForeignIds.MyReportQueueMetrics);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ViewBadge, "xxViewBadge", DefinedRaptorApplicationFunctionForeignIds.ViewBadge);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, "xxViewBadgeLeaderboard", DefinedRaptorApplicationFunctionForeignIds.ViewBadgeLeaderboard);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.AbsenceReport, "xxAbsenceReport", DefinedRaptorApplicationFunctionForeignIds.AbsenceReport);	
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ViewStaffingInfo, "xxViewStaffingInfo", DefinedRaptorApplicationFunctionForeignIds.ViewStaffingInfo);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ViewQRCodeForConfiguration, "xxViewQRCodeForConfiguration", DefinedRaptorApplicationFunctionForeignIds.ViewQRCodeForConfiguration);	

			// Web
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.Anywhere, "xxAnywhere", DefinedRaptorApplicationFunctionForeignIds.Anywhere);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, "xxTeamsModule", DefinedRaptorApplicationFunctionForeignIds.MyTeamSchedules);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.AddFullDayAbsence, "xxAddFullDayAbsence", DefinedRaptorApplicationFunctionForeignIds.AddFullDayAbsence);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.AddIntradayAbsence, "xxAddIntradayAbsence", DefinedRaptorApplicationFunctionForeignIds.AddIntradayAbsence);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.RemoveAbsence, "xxRemoveAbsence", DefinedRaptorApplicationFunctionForeignIds.RemoveAbsence);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.AddActivity, "xxAddActivity", DefinedRaptorApplicationFunctionForeignIds.AddActivity);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.AddPersonalActivity, "xxAddPersonalActivity", DefinedRaptorApplicationFunctionForeignIds.AddPersonalActivity);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.AddOvertimeActivity, "xxAddOvertimeActivity", DefinedRaptorApplicationFunctionForeignIds.AddOvertimeActivity);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.MoveActivity, "xxMoveActivity", DefinedRaptorApplicationFunctionForeignIds.MoveActivity);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.RemoveActivity, "xxRemoveActivity", DefinedRaptorApplicationFunctionForeignIds.RemoveActivity);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.SwapShifts, "xxSwapShifts", DefinedRaptorApplicationFunctionForeignIds.SwapShifts);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.EditShiftCategory, "xxEditShiftCategory", DefinedRaptorApplicationFunctionForeignIds.EditShiftCategory);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.MoveInvalidOverlappedActivity, "xxMoveInvalidOverlappedActivity", DefinedRaptorApplicationFunctionForeignIds.MoveInvalidOverlappedActivity);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, "xxRealTimeAdherenceOverview", DefinedRaptorApplicationFunctionForeignIds.RealTimeAdherenceOverview);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ModifyAdherence, "xxModifyAdherence", DefinedRaptorApplicationFunctionForeignIds.ModifyAdherence);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.HistoricalOverview, "xxHistoricalOverview", DefinedRaptorApplicationFunctionForeignIds.HistoricalOverview);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.AdjustAdherence, "xxAdjustAdherence", DefinedRaptorApplicationFunctionForeignIds.AdjustAdherence);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.WebForecasts, "xxForecasts", DefinedRaptorApplicationFunctionForeignIds.WebForecasts);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.WebPermissions, "xxPermissions", DefinedRaptorApplicationFunctionForeignIds.WebPermissions);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.WebPlans, "xxPlans", DefinedRaptorApplicationFunctionForeignIds.WebPlans);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.WebPeople, "xxPeople", DefinedRaptorApplicationFunctionForeignIds.WebPeople);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.WebRequests, "xxRequests", DefinedRaptorApplicationFunctionForeignIds.WebRequests);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.WebCancelRequest, "xxCancelRequest", DefinedRaptorApplicationFunctionForeignIds.WebCancelRequest);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.WebApproveOrDenyRequest, "xxApproveOrDenyRequest", DefinedRaptorApplicationFunctionForeignIds.WebApproveOrDenyRequest);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.WebReplyRequest, "xxReplyRequest", DefinedRaptorApplicationFunctionForeignIds.WebReplyRequest);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.WebEditSiteOpenHours, "xxEditSiteOpenHours", DefinedRaptorApplicationFunctionForeignIds.WebEditSiteOpenHours);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.WebOvertimeRequest, "xxOvertimeRequests", DefinedRaptorApplicationFunctionForeignIds.WebOvertimeRequest);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.WebModifySkill, "xxModifySkill", DefinedRaptorApplicationFunctionForeignIds.WebModifySkill);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.WebIntraday, "xxIntraday", DefinedRaptorApplicationFunctionForeignIds.WebIntraday);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.WebModifySkillGroup, "xxModifySkillGroup", DefinedRaptorApplicationFunctionForeignIds.WebModifySkillGroup);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.WebStaffing, "xxWebStaffing", DefinedRaptorApplicationFunctionForeignIds.WebStaffing);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.BpoExchange, "xxBpoExchange", DefinedRaptorApplicationFunctionForeignIds.BpoExchange);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.RemoveOvertime, "xxRemoveOvertime", DefinedRaptorApplicationFunctionForeignIds.RemoveOvertime);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.MoveOvertime, "xxMoveOvertime", DefinedRaptorApplicationFunctionForeignIds.MoveOvertime);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.RemoveShift, "xxRemoveShift", DefinedRaptorApplicationFunctionForeignIds.RemoveShift);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.AddDayOff, "xxAddDayOff", DefinedRaptorApplicationFunctionForeignIds.AddDayOff);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.RemoveDayOff, "xxRemoveDayOff", DefinedRaptorApplicationFunctionForeignIds.RemoveDayOff);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.PeopleAccess, "xxAccessManagement", DefinedRaptorApplicationFunctionForeignIds.PeopleAccess);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.PeopleManageUsers, "xxPeopleManageUsers", DefinedRaptorApplicationFunctionForeignIds.PeopleManageUsers);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.Gamification, "xxGamification", DefinedRaptorApplicationFunctionForeignIds.Gamification);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ChatBot, "xxChatBot", DefinedRaptorApplicationFunctionForeignIds.ChatBot);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ExportSchedule, "xxExportSchedule", DefinedRaptorApplicationFunctionForeignIds.ExportSchedule);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.ViewCustomerCenter, "xxViewCustomerCenter", DefinedRaptorApplicationFunctionForeignIds.ViewCustomerCenter);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.SystemSetting, "xxSystemSetting", DefinedRaptorApplicationFunctionForeignIds.SystemSetting);

			// Insights
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.Insights, "Insights", DefinedRaptorApplicationFunctionForeignIds.Insights);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.EditInsightsReport, "xxEditInsightsReport", DefinedRaptorApplicationFunctionForeignIds.EditInsightsReport);
			result.CreateAndAdd(DefinedRaptorApplicationFunctionPaths.DeleteInsightsReport, "xxDeleteInsightsReport", DefinedRaptorApplicationFunctionForeignIds.DeleteInsightsReport);

			return result.ToArray();
		}
	}

	internal static class ApplicationFunctionCollectionExtension
	{
		/// <summary>
		/// Creates a new application function.
		/// </summary>
		/// <param name="applicationFunctionList">The application function list.</param>
		/// <param name="functionPath">The function path.</param>
		/// <param name="functionDescription">The function description.</param>
		/// <param name="definedKey">The foreign GUID id.</param>
		/// <param name="sortOrder"></param>
		public static void CreateAndAdd(this ICollection<IApplicationFunction> applicationFunctionList, string functionPath, string functionDescription, string definedKey, int? sortOrder = null)
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
