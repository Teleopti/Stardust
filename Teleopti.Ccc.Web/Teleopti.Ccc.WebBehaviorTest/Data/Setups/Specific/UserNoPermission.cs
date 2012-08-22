using System.Globalization;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
{
	public class UserNoPermission : IUserSetup
	{
		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			//clear role collection?
			//user.PermissionInformation.ApplicationRoleCollection
		}
	}
}