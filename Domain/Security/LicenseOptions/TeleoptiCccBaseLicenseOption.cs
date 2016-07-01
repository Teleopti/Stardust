using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.LicenseOptions
{
	public class TeleoptiCccBaseLicenseOption : LicenseOption
	{
		public TeleoptiCccBaseLicenseOption()
			: base(DefinedLicenseOptionPaths.TeleoptiCccBase, DefinedLicenseOptionNames.TeleoptiCccBase)
		{
		}

		public override void EnableApplicationFunctions(IEnumerable<IApplicationFunction> allApplicationFunctions)
		{
			var all = allApplicationFunctions.ToList();
			EnabledApplicationFunctions.Clear();
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.All));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.OpenRaptorApplication));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.RaptorGlobal));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.ModifyPersonRestriction));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.OpenSchedulePage));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.OpenForecasterPage));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.AccessToOnlineReports));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.AccessToReports));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.Shifts));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.ModifyPersonNameAndPassword));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.OpenPermissionPage));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.OpenOptionsPage));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.AutomaticScheduling));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.OpenIntradayPage));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.IntradayEarlyWarning));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.MyReportWeb));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.RequestScheduler));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.OpenBudgets));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.ModifyGroupPage));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.ModifyPeopleWithinGroupPage));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.ModifyMeetings));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.SetWriteProtection));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.ViewSchedules));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.ViewConfidential));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.ScheduledTimePerActivityReport));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.ModifySchedule));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.RequestSchedulerApprove));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.AuditTrailSettings));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.ScheduleAuditTrailReport));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.ViewRestrictedScenario));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.ModifyRestrictedScenario));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.ScheduleTimeVersusTargetTimeReport));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.AllowPersonModifications));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.DeletePerson));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.AddPerson));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.ExportForecastToOtherBusinessUnit));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.ExportForecastFile));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.ImportForecastFromFile));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.ExtendedPreferencesWeb));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.ViewActiveAgents));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.MyTimeWeb));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.StudentAvailability));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.StandardPreferences));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.TextRequests));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.MyTimeCancelRequest));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.TeamSchedule));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.ViewAllGroupPages));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.MyReportQueueMetrics));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.Anywhere));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.IntradayReForecasting));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.ModifyAvailabilities));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.ViewBadge));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.AbsenceReport));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.PublishSchedule));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.RequestSchedulerViewAllowance));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.OrganizeCascadingSkills));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboardUnderReports));

			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.WebPermissions));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.WebRequests));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.WebIntraday));
			EnabledApplicationFunctions.Add(ApplicationFunction.FindByPath(all, DefinedRaptorApplicationFunctionPaths.WebModifySkillArea));

		}

	}
}
