using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class RoleForUser : IUserSetup, IUserRoleSetup
	{
		public string Name { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var roleRepository = new ApplicationRoleRepository(uow);
			var role = roleRepository.LoadAll().Single(b => b.Name == Name);
			user.PermissionInformation.AddApplicationRole(role);
		}
	}

}