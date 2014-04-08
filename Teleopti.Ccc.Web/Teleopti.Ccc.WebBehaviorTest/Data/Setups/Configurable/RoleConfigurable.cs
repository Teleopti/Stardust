using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Common;
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
		public string BusinessUnit { get; set; }

		public string AccessToTeam { get; set; }
		public bool AccessToMyOwn { get; set; }
		public bool NoDataAccess { get; set; }

		public bool ViewUnpublishedSchedules { get; set; }
		public bool ViewConfidential { get; set; }
		public bool AccessToMobileReports { get; set; }
		public bool AccessToExtendedPreferences { get; set; }
		public bool AccessToMytimeWeb { get; set; }
		public bool AccessToAnywhere { get; set; }
		public bool AccessToAsm { get; set; }
		public bool AccessToTextRequests { get; set; }
		public bool AccessToAbsenceRequests { get; set; }
		public bool AccessToShiftTradeRequests { get; set; }
		public bool AccessToStudentAvailability { get; set; }
		public bool AccessToCalendarLink { get; set; }
		public bool AccessToOvertimeAvailability { get; set; }
		public bool AccessToMyReport { get; set; }
		public bool AccessToUnderConstruction { get; set; }
		public bool AccessToPreferences { get; set; }
		public bool AccessToRealTimeAdherenceOverview { get; set; }
		public bool AccessToTeamSchedule { get; set; }
		public bool AccessToViewAllGroupPages { get; set; }
		public bool AccessToMatrixReports { get; set; }
		public bool AccessToPersonalAbsenceAccount { get; set; }
		public bool CanLogonAsAnotherUser { get; set; }

		public RoleConfigurable()
		{
			Name = DefaultName.Make("A role");
			BusinessUnit = GlobalDataMaker.Data().Data<CommonBusinessUnit>().BusinessUnit.Description.Name;
			ViewUnpublishedSchedules = false;
			ViewConfidential = false;
			AccessToMyOwn = false;
			NoDataAccess = false;
			AccessToMobileReports = false;
			AccessToExtendedPreferences = true;
			AccessToMytimeWeb = true;
			AccessToAsm = true;
			AccessToTextRequests = true;
			AccessToAbsenceRequests = true;
			AccessToShiftTradeRequests = true;
			AccessToAnywhere = false;
			AccessToViewAllGroupPages = false;
			AccessToCalendarLink = false;
			AccessToOvertimeAvailability = false;
			AccessToMyReport = true;
			AccessToUnderConstruction = true;
			AccessToPreferences = true;
			AccessToRealTimeAdherenceOverview = false;
			AccessToTeamSchedule = true;
			AccessToMatrixReports = true;
			AccessToPersonalAbsenceAccount = true;
			CanLogonAsAnotherUser = true;
		}

		public void Apply(IUnitOfWork uow)
		{
			var role = ApplicationRoleFactory.CreateRole(Name, null);

			var availableDataRangeOption = NoDataAccess
														 ? AvailableDataRangeOption.None
														 : AccessToMyOwn ? AvailableDataRangeOption.MyOwn : AvailableDataRangeOption.MyTeam;
			var availableData = new AvailableData
			{
				ApplicationRole = role,
				AvailableDataRange = availableDataRangeOption
			};
			role.AvailableData = availableData;


			if (!string.IsNullOrEmpty(AccessToTeam))
			{
				var teamRepository = new TeamRepository(uow);
				AccessToTeam.Split(',')
					.Select(t => teamRepository.FindTeamByDescriptionName(t.Trim()).Single())
					.ForEach(role.AvailableData.AddAvailableTeam);
			}

			var businessUnitRepository = new BusinessUnitRepository(uow);
			var businessUnit = businessUnitRepository.LoadAllBusinessUnitSortedByName().Single(b => b.Name == BusinessUnit);
			role.SetBusinessUnit(businessUnit);

			var applicationFunctionRepository = new ApplicationFunctionRepository(uow);
			var allApplicationFunctions = applicationFunctionRepository.LoadAll();
			var filteredApplicationFunctions = FilterApplicationFunctions(allApplicationFunctions);
			filteredApplicationFunctions.ToList().ForEach(role.AddApplicationFunction);

			var applicationRoleRepository = new ApplicationRoleRepository(uow);
			var availableDataRepository = new AvailableDataRepository(uow);

			applicationRoleRepository.Add(role);
			availableDataRepository.Add(availableData);

		}

		/// <summary>
		/// Filters the application functions by the filter properties.
		/// </summary>
		/// <param name="allApplicationFunctions">All application functions.</param>
		/// <returns>Filtered application functions</returns>
		private IEnumerable<IApplicationFunction> FilterApplicationFunctions(IList<IApplicationFunction> allApplicationFunctions)
		{
			var applicationFunctions = from f in allApplicationFunctions
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

			if (!AccessToMobileReports)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.MobileReports
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
			if (!AccessToUnderConstruction)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.UnderConstruction
											  select f;
			if (!AccessToRealTimeAdherenceOverview)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview
											  select f;
			if (!AccessToPersonalAbsenceAccount)
				applicationFunctions = from f in applicationFunctions
											  where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.ViewPersonalAccount
											  select f;
			if (!AccessToMatrixReports)
				applicationFunctions = from f in applicationFunctions
											  where f.ForeignSource != DefinedForeignSourceNames.SourceMatrix
											  select f;
			if (!CanLogonAsAnotherUser)
				applicationFunctions = from f in applicationFunctions
									   where f.FunctionPath != DefinedRaptorApplicationFunctionPaths.SignInAsAnotherUser
									   select f;
			return applicationFunctions;
		}
	}
}