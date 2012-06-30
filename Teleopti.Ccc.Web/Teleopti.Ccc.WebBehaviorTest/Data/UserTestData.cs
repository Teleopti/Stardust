using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class UserTestData
	{
		public static IPerson AgentWindowsUser;

		public static IPerson AgentApplicationUser;
		public static string AgentApplicationUserName = "applicationUser";
		public static IPerson AgentApplicationUserSingleBusinessUnit;
		public static string AgentApplicationUserSingleBusinessUnitUserName = "applicationUserSingleBusinessUnit";

		public static IPerson UserWithNoPermission;
		public static string UserWithNoPermissionUserName = "noPermission";
	}
}