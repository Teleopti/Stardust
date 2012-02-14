using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class UserTestData
	{
		public static IPerson PersonWindowsUser;

		public static IPerson PersonApplicationUser;
		public static string PersonApplicationUserName = "applicationUser";
		public static IPerson PersonApplicationUserSingleBusinessUnit;
		public static string PersonApplicationUserSingleBusinessUnitUserName = "applicationUserSingleBusinessUnit";

		public static IPerson PersonWithNoPermission;
		public static string PersonWithNoPermissionUserName = "noPermission";
	}
}