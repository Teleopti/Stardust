using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific
{
	public class AgentWithoutExtendedPreferencesAccess : IUserSetup, IUserRoleSetup
	{
		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			user.PermissionInformation.AddApplicationRole(TestData.AgentRoleWithoutExtendedPreferences);
		}
	}
}