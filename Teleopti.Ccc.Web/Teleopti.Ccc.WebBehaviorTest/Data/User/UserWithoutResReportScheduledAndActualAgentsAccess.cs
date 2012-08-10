namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	using System.Globalization;

	using Interfaces;
	using Teleopti.Interfaces.Domain;

	public class UserWithoutResReportScheduledAndActualAgentsAccess : IUserSetup
	{
		public void Apply(IPerson user, CultureInfo cultureInfo)
		{
			user.PermissionInformation.AddApplicationRole(TestData.AgentRoleWithoutResReportScheduledAndActualAgents);
		}
	}
}