namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	using System.Globalization;

	using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
	using Teleopti.Interfaces.Domain;

	public class UserWithoutResReportServiceLevelAndAgentsReadyAccess : IUserSetup
	{
		public void Apply(IPerson user, CultureInfo cultureInfo)
		{
			user.PermissionInformation.AddApplicationRole(TestData.AgentRoleWithoutResReportServiceLevelAndAgentsReady);
		}
	}
}