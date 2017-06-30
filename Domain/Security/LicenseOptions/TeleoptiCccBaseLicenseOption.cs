﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

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
			var appFunctionPaths = new List<string>
			{
				DefinedRaptorApplicationFunctionPaths.All,
				DefinedRaptorApplicationFunctionPaths.OpenRaptorApplication,
				DefinedRaptorApplicationFunctionPaths.RaptorGlobal,
				DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence,
				DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment,
				DefinedRaptorApplicationFunctionPaths.ModifyPersonRestriction,
				DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules,
				DefinedRaptorApplicationFunctionPaths.OpenSchedulePage,
				DefinedRaptorApplicationFunctionPaths.OpenForecasterPage,
				DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage,
				DefinedRaptorApplicationFunctionPaths.AccessToOnlineReports,
				DefinedRaptorApplicationFunctionPaths.AccessToReports,
				DefinedRaptorApplicationFunctionPaths.Shifts,
				DefinedRaptorApplicationFunctionPaths.ModifyPersonNameAndPassword,
				DefinedRaptorApplicationFunctionPaths.OpenPermissionPage,
				DefinedRaptorApplicationFunctionPaths.OpenOptionsPage,
				DefinedRaptorApplicationFunctionPaths.AutomaticScheduling,
				DefinedRaptorApplicationFunctionPaths.OpenIntradayPage,
				DefinedRaptorApplicationFunctionPaths.IntradayEarlyWarning,
				DefinedRaptorApplicationFunctionPaths.MyReportWeb,
				DefinedRaptorApplicationFunctionPaths.RequestScheduler,
				DefinedRaptorApplicationFunctionPaths.OpenBudgets,
				DefinedRaptorApplicationFunctionPaths.ModifyGroupPage,
				DefinedRaptorApplicationFunctionPaths.ModifyPeopleWithinGroupPage,
				DefinedRaptorApplicationFunctionPaths.ModifyMeetings,
				DefinedRaptorApplicationFunctionPaths.SetWriteProtection,
				DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule,
				DefinedRaptorApplicationFunctionPaths.ViewSchedules,
				DefinedRaptorApplicationFunctionPaths.ViewConfidential,
				DefinedRaptorApplicationFunctionPaths.ScheduledTimePerActivityReport,
				DefinedRaptorApplicationFunctionPaths.ModifySchedule,
				DefinedRaptorApplicationFunctionPaths.RequestSchedulerApprove,
				DefinedRaptorApplicationFunctionPaths.AuditTrailSettings,
				DefinedRaptorApplicationFunctionPaths.ScheduleAuditTrailReport,
				DefinedRaptorApplicationFunctionPaths.ViewRestrictedScenario,
				DefinedRaptorApplicationFunctionPaths.ModifyRestrictedScenario,
				DefinedRaptorApplicationFunctionPaths.ScheduleTimeVersusTargetTimeReport,
				DefinedRaptorApplicationFunctionPaths.AllowPersonModifications,
				DefinedRaptorApplicationFunctionPaths.DeletePerson,
				DefinedRaptorApplicationFunctionPaths.AddPerson,
				DefinedRaptorApplicationFunctionPaths.ExportForecastToOtherBusinessUnit,
				DefinedRaptorApplicationFunctionPaths.ExportForecastFile,
				DefinedRaptorApplicationFunctionPaths.ImportForecastFromFile,
				DefinedRaptorApplicationFunctionPaths.ExtendedPreferencesWeb,
				DefinedRaptorApplicationFunctionPaths.ViewActiveAgents,
				DefinedRaptorApplicationFunctionPaths.MyTimeWeb,
				DefinedRaptorApplicationFunctionPaths.StudentAvailability,
				DefinedRaptorApplicationFunctionPaths.StandardPreferences,
				DefinedRaptorApplicationFunctionPaths.TextRequests,
				DefinedRaptorApplicationFunctionPaths.MyTimeCancelRequest,
				DefinedRaptorApplicationFunctionPaths.TeamSchedule,
				DefinedRaptorApplicationFunctionPaths.ViewAllGroupPages,
				DefinedRaptorApplicationFunctionPaths.MyReportQueueMetrics,
				DefinedRaptorApplicationFunctionPaths.Anywhere,
				DefinedRaptorApplicationFunctionPaths.IntradayReForecasting,
				DefinedRaptorApplicationFunctionPaths.ModifyAvailabilities,
				DefinedRaptorApplicationFunctionPaths.ViewBadge,
				DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard,
				DefinedRaptorApplicationFunctionPaths.AbsenceReport,
				DefinedRaptorApplicationFunctionPaths.PublishSchedule,
				DefinedRaptorApplicationFunctionPaths.RequestSchedulerViewAllowance,
				DefinedRaptorApplicationFunctionPaths.OrganizeCascadingSkills,
				DefinedRaptorApplicationFunctionPaths.ArchiveSchedule,
				DefinedRaptorApplicationFunctionPaths.ImportSchedule,
				DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboardUnderReports,
				DefinedRaptorApplicationFunctionPaths.ViewStaffingInfo,
				DefinedRaptorApplicationFunctionPaths.ViewQRCodeForConfiguration,
				DefinedRaptorApplicationFunctionPaths.OvertimeRequestWeb,

				DefinedRaptorApplicationFunctionPaths.WebPermissions,
				DefinedRaptorApplicationFunctionPaths.WebRequests,
				DefinedRaptorApplicationFunctionPaths.WebIntraday,
				DefinedRaptorApplicationFunctionPaths.WebPeople,
				DefinedRaptorApplicationFunctionPaths.WebModifySkillArea,
				DefinedRaptorApplicationFunctionPaths.WebStaffing,
				DefinedRaptorApplicationFunctionPaths.SaveFavoriteSearch

			};

			var webTeamsFunctionPaths = new List<string>
			{
				DefinedRaptorApplicationFunctionPaths.MyTeamSchedules,
				DefinedRaptorApplicationFunctionPaths.AddFullDayAbsence,
				DefinedRaptorApplicationFunctionPaths.AddIntradayAbsence,
				DefinedRaptorApplicationFunctionPaths.RemoveAbsence,
				DefinedRaptorApplicationFunctionPaths.AddActivity,
				DefinedRaptorApplicationFunctionPaths.AddPersonalActivity,
				DefinedRaptorApplicationFunctionPaths.AddOvertimeActivity,
				DefinedRaptorApplicationFunctionPaths.MoveActivity,
				DefinedRaptorApplicationFunctionPaths.RemoveActivity,
				DefinedRaptorApplicationFunctionPaths.SwapShifts,
				DefinedRaptorApplicationFunctionPaths.EditShiftCategory,
				DefinedRaptorApplicationFunctionPaths.MoveInvalidOverlappedActivity,
				DefinedRaptorApplicationFunctionPaths.RemoveOvertime,
				DefinedRaptorApplicationFunctionPaths.MoveOvertime

			};

			webTeamsFunctionPaths.ForEach(appFunctionPaths.Add);

			var all = allApplicationFunctions.ToList();
			EnableFunctions(appFunctionPaths.Select(appFunctionPath => ApplicationFunction.FindByPath(all, appFunctionPath)).ToArray());
		}
	}
}
