using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	/// <summary>
	/// Creates a role, functions, available data structure for webtests.
	/// </summary>
	/// <remarks>
	/// Creates a role given by the Name propery and loads the application functions.
	/// There is no check method for ungiven Name property. 
	/// </remarks>

	// DONT default permissions to false!
	// False is the edge case!
	public class RoleConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string BusinessUnit { get; set; }

		public string AccessToTeam { get; set; }
		public string AccessToSite { get; set; }
		public bool AccessToMyOwn { get; set; }
		public bool AccessToMySite { get; set; }
		public bool AccessToEveryone { get; set; }
		public bool NoDataAccess { get; set; }

		public bool AccessToEverything { get; set; }
		public bool ViewUnpublishedSchedules { get; set; }
		public bool ViewConfidential { get; set; }
		public bool AccessToExtendedPreferences { get; set; }
		public bool AccessToMytimeWeb { get; set; }
		public bool AccessToAnywhere { get; set; }
		public bool AccessToSeatPlanner { get; set; }
		public bool AccessToAsm { get; set; }
		public bool AccessToTextRequests { get; set; }
		public bool AccessToAbsenceRequests { get; set; }
		public bool AccessToOvertimeRequests { get; set; }
		public bool AccessToAbsenceReport { get; set; }
		public bool AccessToShiftTradeRequests { get; set; }
		public bool AccessToShiftTradeBulletinBoard { get; set; }
		public bool AccessToStudentAvailability { get; set; }
		public bool AccessToCalendarLink { get; set; }
		public bool AccessToOvertimeAvailability { get; set; }
		public bool AccessToMyReport { get; set; }
		public bool AccessToPreferences { get; set; }
		public bool AccessToRealTimeAdherenceOverview { get; set; }
		public bool AccessToModifyAdherence { get; set; }
		public bool AccessToHistoricalOverview { get; set; }
		public bool AccessToTeamSchedule { get; set; }
		public bool AccessToViewAllGroupPages { get; set; }
		public bool AccessToMatrixReports { get; set; }
		public bool AccessToPersonalAbsenceAccount { get; set; }
		public bool AccessToMyReportQueueMetrics { get; set; }
		public bool AccessToLeaderboard { get; set; }
		public bool AccessToResourcePlanner { get; set; }
		public bool AccessToPeople { get; set; }
		public bool AccessToOutbound { get; set; }
		public bool AccessToWfmRequests { get; set; }
		public bool AccessToWfmMyTeamSchedule { get; set; }
		public bool AccessToWfmLeaderboard { get; set; }
		public bool AccessToStaffing { get; set; }

		public bool AddFullDayAbsence { get; set; }
		public bool AddIntradayAbsence { get; set; }
		public bool RemoveAbsence { get; set; }
		public bool AddActivity { get; set; }
		public bool MoveActivity { get; set; }
		public bool RemoveActivity { get; set; }
		public bool CopySchedules { get; set; }
		public bool ImportSchedules { get; set; }

		public bool QuickForecaster { get; set; }
		public bool AccessToIntraday { get; set; }

		public bool AccessToPermissions { get; set; }
		
		public bool AccessToModifySkillGroup { get; set; }

		public RoleConfigurable()
		{
			// DONT default permissions to false!
			// False is the edge case!
			Name = RandomName.Make("A role");
			Description = RandomName.Make("A role");
			BusinessUnit = DefaultBusinessUnit.BusinessUnit.Description.Name;
			AccessToEverything = false;
			ViewUnpublishedSchedules = false;
			ViewConfidential = false;
			AccessToMyOwn = false;
			AccessToMySite = false;
			NoDataAccess = false;
			AccessToExtendedPreferences = true;
			AccessToMytimeWeb = true;
			AccessToAsm = true;
			AccessToTextRequests = true;
			AccessToAbsenceRequests = true;
			AccessToOvertimeRequests = true;
			AccessToAbsenceReport = true;
			AccessToShiftTradeRequests = true;
			AccessToShiftTradeBulletinBoard = true;
			AccessToAnywhere = false;
			AccessToSeatPlanner = false;
			AccessToViewAllGroupPages = false;
			AccessToCalendarLink = false;
			AccessToOvertimeAvailability = false;
			AccessToMyReport = true;
			AccessToPreferences = true;
			AccessToRealTimeAdherenceOverview = true;
			AccessToModifyAdherence = true;
			AccessToHistoricalOverview = true;
			AccessToSeatPlanner = false;
			AccessToTeamSchedule = true;
			AccessToMatrixReports = true;
			AccessToPersonalAbsenceAccount = true;
			AccessToMyReportQueueMetrics = true;
			AccessToLeaderboard = true;
			AccessToStaffing = true;
			AddFullDayAbsence = true;
			AddIntradayAbsence = true;
			RemoveAbsence = true;
			AddActivity = true;
			MoveActivity = true;
			RemoveActivity = true;
			QuickForecaster = false;
			AccessToOutbound = false;
			AccessToWfmRequests = false;
			AccessToWfmMyTeamSchedule = false;
			AccessToPermissions = false;
			AccessToWfmLeaderboard = true;
			AccessToModifySkillGroup = true;
			AccessToIntraday = false;
		}


		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var role = ApplicationRoleFactory.CreateRole(Name, Description);

			var availableDataRangeOption = NoDataAccess
				? AvailableDataRangeOption.None
				: AccessToEveryone
					? AvailableDataRangeOption.Everyone
					: AccessToMyOwn
						? AvailableDataRangeOption.MyOwn
						: AccessToMySite
							? AvailableDataRangeOption.MySite
							: AvailableDataRangeOption.MyTeam;
			var availableData = new AvailableData
			{
				ApplicationRole = role,
				AvailableDataRange = availableDataRangeOption
			};
			role.AvailableData = availableData;


			if (!string.IsNullOrEmpty(AccessToTeam))
			{
				var teamRepository = new TeamRepository(currentUnitOfWork);
				AccessToTeam.Split(',')
					.Select(t => teamRepository.FindTeamByDescriptionName(t.Trim()).Single())
					.ForEach(role.AvailableData.AddAvailableTeam);
			}

			if (!string.IsNullOrEmpty(AccessToSite))
			{
				var siteRepository = new SiteRepository(currentUnitOfWork);
				AccessToSite.Split(',')
					.Select(s => siteRepository.FindSiteByDescriptionName(s.Trim()).Single())
					.ForEach(role.AvailableData.AddAvailableSite);
			}

			var businessUnitRepository = new BusinessUnitRepository(currentUnitOfWork);
			var businessUnit = businessUnitRepository.LoadAllBusinessUnitSortedByName().Single(b => b.Name == BusinessUnit);
			role.SetBusinessUnit(businessUnit);

			var applicationFunctionRepository = new ApplicationFunctionRepository(currentUnitOfWork);
			var allApplicationFunctions = applicationFunctionRepository.LoadAll();
			var filteredApplicationFunctions = filterApplicationFunctions(allApplicationFunctions);
			filteredApplicationFunctions.ToList().ForEach(role.AddApplicationFunction);

			var applicationRoleRepository = new ApplicationRoleRepository(currentUnitOfWork);
			var availableDataRepository = new AvailableDataRepository(currentUnitOfWork);

			applicationRoleRepository.Add(role);
			availableDataRepository.Add(availableData);
		}

		private IEnumerable<IApplicationFunction> filterApplicationFunctions(IEnumerable<IApplicationFunction> allApplicationFunctions)
		{
			IEnumerable<IApplicationFunction> applicationFunctions;
			if (AccessToEverything)
				applicationFunctions = from f in allApplicationFunctions
					where f.FunctionPath == DefinedRaptorApplicationFunctionPaths.All
					select f;
			else
				applicationFunctions =
					allApplicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.All);

			if (!ViewUnpublishedSchedules)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);

			if (!ViewConfidential)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.ViewConfidential);

			if (!AccessToExtendedPreferences)
				applicationFunctions =
					applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.ExtendedPreferencesWeb);

			if (!AccessToMytimeWeb)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.MyTimeWeb);
			if (!AccessToAsm)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger);
			if (!AccessToTextRequests)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.TextRequests);
			if (!AccessToAbsenceRequests)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb);
			if (!AccessToOvertimeRequests)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.OvertimeRequestWeb);
			if (!AccessToAbsenceReport)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.AbsenceReport);
			if (!AccessToAnywhere)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.Anywhere);
			if (!AccessToStudentAvailability)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.StudentAvailability);
			if (!AccessToShiftTradeRequests)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb);
			if (!AccessToShiftTradeBulletinBoard)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.ShiftTradeBulletinBoard);
			if (!AccessToViewAllGroupPages)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.ViewAllGroupPages);
			if (!AccessToCalendarLink)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.ShareCalendar);
			if (!AccessToOvertimeAvailability)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.OvertimeAvailabilityWeb);
			if (!AccessToPreferences)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.StandardPreferences);
			if (!AccessToTeamSchedule)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.TeamSchedule);
			if (!AccessToMyReport)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.MyReportWeb);

			if (!AccessToRealTimeAdherenceOverview)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview);
			if (!AccessToModifyAdherence)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.ModifyAdherence);
			if (!AccessToHistoricalOverview)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.HistoricalOverview);
			
			if (!AccessToResourcePlanner)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.WebPlans);
			if (!AccessToPeople)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.WebPeople);

			if (!AccessToOutbound)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.Outbound);

			if (!AccessToPersonalAbsenceAccount)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.ViewPersonalAccount);
			if (!AccessToMatrixReports)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.AccessToReports);
			if (!AccessToMatrixReports)
				applicationFunctions = applicationFunctions.Except(f => f.ForeignSource == DefinedForeignSourceNames.SourceMatrix);
			if (!AccessToMyReportQueueMetrics)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.MyReportQueueMetrics);
			if (!AccessToLeaderboard)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard);

			if (!AddFullDayAbsence)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.AddFullDayAbsence);
			if (!AddIntradayAbsence)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.AddIntradayAbsence);
			if (!RemoveAbsence)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.RemoveAbsence);
			if (!AddActivity)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.AddActivity);
			if (!RemoveActivity)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.RemoveActivity);
			if (!MoveActivity)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.MoveActivity);
			if (!QuickForecaster)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.WebForecasts);

			if (!AccessToSeatPlanner)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.SeatPlanner);

			if (!AccessToWfmRequests)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.WebRequests);

			if (!AccessToWfmMyTeamSchedule && !AccessToAnywhere)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);

			if (!AccessToIntraday)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.WebIntraday);

			if (!AccessToPermissions)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.WebPermissions);
			if (!AccessToWfmLeaderboard)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboardUnderReports);
			if (!AccessToStaffing)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.WebStaffing);

			if (!CopySchedules)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.CopySchedule);

			if (!ImportSchedules)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.ImportSchedule);

			if (!AccessToModifySkillGroup)
				applicationFunctions = applicationFunctions.Except(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.WebModifySkillGroup);
			
			return applicationFunctions;
		}
	}
}