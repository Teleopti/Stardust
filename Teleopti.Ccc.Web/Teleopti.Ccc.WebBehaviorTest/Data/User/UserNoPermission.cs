using System.Globalization;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class UserNoPermission : IUserSetup
	{
		public void Apply(IPerson user, CultureInfo cultureInfo)
		{
			//clear role collection?
			//user.PermissionInformation.ApplicationRoleCollection
		}
	}
}