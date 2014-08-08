using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class TestData
	{

		public static IDataSource DataSource;
		public static IBusinessUnit BusinessUnit { get { return CommonBusinessUnit.BusinessUnitFromFakeState; } }
		
		public static IPerson PersonThatCreatesTestData;

		public static string CommonPassword = "1";

		public static IApplicationRole AdministratorRole;
		public static IApplicationRole AgentRole;
		public static IApplicationRole UnitRole;
		public static IApplicationRole SiteRole;
		public static IApplicationRole TeamRole;
		public static IApplicationRole AgentRoleSecondBusinessUnit;
		public static IApplicationRole AgentRoleWithoutStudentAvailability;
		public static IApplicationRole AgentRoleWithoutPreferences;
		public static IApplicationRole AgentRoleWithoutExtendedPreferences;
		public static IApplicationRole AgentRoleWithoutRequests;
		public static IApplicationRole AgentRoleWithoutAbsenceRequests;
		public static IApplicationRole AgentRoleWithoutTeamSchedule;
		public static IApplicationRole AgentRoleWithoutMyTimeWeb;
		public static IApplicationRole AgentRoleWithoutAnyReport;
		public static IApplicationRole SupervisorRole;
		public static IApplicationRole SupervisorRoleSecondBusinessUnit;

		public static IApplicationRole AgentRoleWithoutResReportScheduledAndActualAgents;
		
		public static IApplicationRole AgentRoleOnlyWithOwnData;
		public static IApplicationRole AgentRoleWithSiteData;
		public static IApplicationRole AgentRoleWithAnotherSiteData;
		public static IApplicationRole AdministratorRoleWithEveryoneData;

		public static IActivity ActivityPhone;
		public static IActivity ActivityShortBreak;
		public static IActivity ActivityLunch;
		public static IActivity ActivityTraining;

		public static IShiftCategory ShiftCategory;
		public static IDayOffTemplate DayOffTemplate;
		public static IAbsence Absence;
		public static IAbsence ConfidentialAbsence;
		public static IAbsence AbsenceInContractTime;
	}
}
