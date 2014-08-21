﻿using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class TestData
	{
		public static IBusinessUnit BusinessUnit { get { return CommonBusinessUnit.BusinessUnitFromFakeState; } }

		public static string CommonPassword = "1";

		public static IApplicationRole AgentRole;
		public static IApplicationRole AgentRoleWithoutStudentAvailability;
		public static IApplicationRole AgentRoleWithoutPreferences;
		public static IApplicationRole AgentRoleWithoutExtendedPreferences;
		public static IApplicationRole AgentRoleWithoutRequests;
		public static IApplicationRole AgentRoleWithoutAbsenceRequests;
		public static IApplicationRole AgentRoleWithoutTeamSchedule;
		
		public static IApplicationRole AgentRoleOnlyWithOwnData;
		public static IApplicationRole AgentRoleWithSiteData;
		public static IApplicationRole AgentRoleWithAnotherSiteData;
		public static IApplicationRole AdministratorRoleWithEveryoneData;

		public static IShiftCategory ShiftCategory;
		public static IAbsence ConfidentialAbsence;
		public static IAbsence AbsenceInContractTime;
	}
}
