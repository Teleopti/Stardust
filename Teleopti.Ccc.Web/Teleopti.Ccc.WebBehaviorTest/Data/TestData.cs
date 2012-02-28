using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class TestData
	{

		public static IDataSource DataSource;

		public static IPerson PersonThatCreatesTestData;

		public static string CommonPassword = "1";

		public static IBusinessUnit BusinessUnit;
		public static IBusinessUnit SecondBusinessUnit;

		public static IApplicationRole AdministratorRole;
		public static IApplicationRole AgentRole;
		public static IApplicationRole UnitRole;
		public static IApplicationRole SiteRole;
		public static IApplicationRole TeamRole;
		public static IApplicationRole AgentRoleSecondBusinessUnit;
		public static IApplicationRole AgentRoleWithoutStudentAvailability;
		public static IApplicationRole AgentRoleWithoutPreferences;
		public static IApplicationRole AgentRoleWithoutRequests;
		public static IApplicationRole AgentRoleWithoutTextRequests;
		public static IApplicationRole AgentRoleWithoutTeamSchedule;
		public static IApplicationRole AgentRoleWithoutMobileReports;
		public static IApplicationRole AgentRoleWithoutMyTimeWeb;

		public static IApplicationRole AgentRoleWithoutResReportServiceLevelAndAgentsReady;
		
		public static IApplicationRole AgentRoleOnlyWithOwnData;
		public static IApplicationRole AgentRoleWithSiteData;
		public static IApplicationRole AgentRoleWithAnotherSiteData;

		public static IActivity ActivityPhone;
		public static IActivity ActivityLunch;
		public static IActivity ActivityTraining;

		public static IWorkflowControlSet WorkflowControlSetExisting;
		public static IWorkflowControlSet WorkflowControlSetPublished;
		public static IWorkflowControlSet WorkflowControlSetPublishedUntilWednesday;
		public static IWorkflowControlSet WorkflowControlSetNotPublished;
		public static IWorkflowControlSet WorkflowControlSetStudentAvailabilityOpen;
		public static IWorkflowControlSet WorkflowControlSetStudentAvailabilityClosed;
		public static IWorkflowControlSet WorkflowControlSetStudentAvailabilityOpenNextMonth;
		public static IWorkflowControlSet WorkflowControlSetPreferenceOpen;
		public static IWorkflowControlSet WorkflowControlSetPreferenceOpenWithAllowedPreferences;
		public static IWorkflowControlSet WorkflowControlSetPreferencesOpenNextMonth;
		public static IWorkflowControlSet WorkflowControlSetPreferenceClosed;

		public static IScenario Scenario;
		public static IGroupingActivity GroupingActivity;
		public static IShiftCategory ShiftCategory;
		public static IDayOffTemplate DayOffTemplate;
		public static IAbsence Absence;
		public static IAbsence ConfidentialAbsence;


		public static ITeam CommonTeam;
		public static ISite CommonSite;
		public static ISite AnotherSite;

		public static IContract ContractOne;
		public static IPartTimePercentage PartTimePercentageOne;
		public static IContractSchedule ContractScheduleOne;

	}
}
