using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Default;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	/// <summary>
	/// Creates a role, functions, available data structure for webtests.
	/// </summary>
	/// <remarks>
	/// Creates a role given by the Name propery and loads the application functions.
	/// There is no check method for ungiven Name property. 
	/// </remarks>
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
		public bool AccessToAbsenceReport { get; set; }
		public bool AccessToShiftTradeRequests { get; set; }
		public bool AccessToShiftTradeBulletinBoard { get; set; }
		public bool AccessToStudentAvailability { get; set; }
		public bool AccessToCalendarLink { get; set; }
		public bool AccessToOvertimeAvailability { get; set; }
		public bool AccessToMyReport { get; set; }
		public bool AccessToPreferences { get; set; }
		public bool AccessToRealTimeAdherenceOverview { get; set; }
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

		public bool AddFullDayAbsence { get; set; }
		public bool AddIntradayAbsence { get; set; }
		public bool RemoveAbsence { get; set; }
		public bool AddActivity { get; set; }
		public bool MoveActivity { get; set; }

		public bool QuickForecaster { get; set; }

		public RoleConfigurable()
		{
			// DONT default permissions to false!
			// False is the edge case!
			Name = RandomName.Make("A role");
			Description = RandomName.Make("A role");
			BusinessUnit = DefaultBusinessUnit.BusinessUnitFromFakeState.Description.Name;
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
			AccessToSeatPlanner = false;
			AccessToTeamSchedule = true;
			AccessToMatrixReports = true;
			AccessToPersonalAbsenceAccount = true;
			AccessToMyReportQueueMetrics = true;
			AccessToLeaderboard = true;
			AddFullDayAbsence = true;
			AddIntradayAbsence = true;
			RemoveAbsence = true;
			AddActivity = true;
			MoveActivity = true;
			QuickForecaster = false;
			AccessToOutbound = false;
			AccessToWfmRequests = false;
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
				applicationFunctions = from f in allApplicationFunctions
					where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.All
					select f;

			if (!ViewUnpublishedSchedules)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules
											  select f;

			if (!ViewConfidential)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.ViewConfidential
											  select f;

			if (!AccessToExtendedPreferences)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.ExtendedPreferencesWeb
											  select f;
			if (!AccessToMytimeWeb)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.MyTimeWeb
											  select f;
			if (!AccessToAsm)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger
											  select f;
			if (!AccessToTextRequests)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.TextRequests
											  select f;
			if (!AccessToAbsenceRequests)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb
											  select f;
			if (!AccessToAbsenceReport)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.AbsenceReport
											  select f;
			if (!AccessToAnywhere)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.Anywhere
											  select f;
			if (!AccessToStudentAvailability)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.StudentAvailability
											  select f;
			if (!AccessToShiftTradeRequests)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb
											  select f;
			if (!AccessToShiftTradeBulletinBoard)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.ShiftTradeBulletinBoard
											  select f;
			if (!AccessToViewAllGroupPages)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.ViewAllGroupPages
											  select f;
			if (!AccessToCalendarLink)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.ShareCalendar
											  select f;
			if (!AccessToOvertimeAvailability)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.OvertimeAvailabilityWeb
											  select f;
			if (!AccessToPreferences)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.StandardPreferences
											  select f;
			if (!AccessToTeamSchedule)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.TeamSchedule
											  select f;
			if (!AccessToMyReport)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.MyReportWeb
											  select f;
			if (!AccessToRealTimeAdherenceOverview)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview
											  select f;
			if (!AccessToResourcePlanner)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.WebSchedules
											  select f;
			if (!AccessToPeople)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.WebPeople
											  select f;

			if (!AccessToOutbound)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.Outbound
											  select f;

			if (!AccessToPersonalAbsenceAccount)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.ViewPersonalAccount
											  select f;
			if (!AccessToMatrixReports)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.AccessToReports
											  select f;
			if (!AccessToMatrixReports)
				applicationFunctions = from f in applicationFunctions
											  where f.ForeignSource != DefinedForeignSourceNames.SourceMatrix
											  select f;
			if (!AccessToMyReportQueueMetrics)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.MyReportQueueMetrics
											  select f;
			if (!AccessToLeaderboard)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard
											  select f;

			if (!AddFullDayAbsence)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.AddFullDayAbsence
											  select f;
			if (!AddIntradayAbsence)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.AddIntradayAbsence
											  select f;
			if (!RemoveAbsence)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.RemoveAbsence
											  select f;
			if (!AddActivity)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.AddActivity
											  select f;
			if (!MoveActivity)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.MoveActivity
											  select f;
			if (!QuickForecaster)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.WebForecasts
											  select f;

			if (!AccessToSeatPlanner)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.SeatPlanner
											  select f;

			if (!AccessToWfmRequests)
				applicationFunctions = from f in applicationFunctions
									   where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.WebRequests
									   select f;

			return applicationFunctions;
		}
	}
}