namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	using System.Globalization;

	using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
	using Teleopti.Interfaces.Domain;

	public class Supervisor : IUserSetup
	{
		#region IUserSetup Members

		public void Apply(IPerson user, CultureInfo cultureInfo)
		{
			// TODO: Use TestData.AgentRoleWithoutMyTimeWeb When logOnOff handles this
			user.PermissionInformation.AddApplicationRole(TestData.SupervisorRole);
		}

		#endregion
	}
}