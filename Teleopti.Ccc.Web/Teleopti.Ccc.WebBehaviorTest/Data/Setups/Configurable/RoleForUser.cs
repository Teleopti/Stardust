using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class RoleForUser : IUserSetup, IUserRoleSetup
	{
		public string Name { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var roleRepository = ApplicationRoleRepository.DONT_USE_CTOR(uow);
			var role = roleRepository.LoadAll().Single(b => b.Name == Name);
			user.PermissionInformation.AddApplicationRole(role);
		}
	}

}