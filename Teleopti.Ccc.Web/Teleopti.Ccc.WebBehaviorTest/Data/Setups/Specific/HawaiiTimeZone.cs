using System.Globalization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
{
	public class HawaiiTimeZone : IUserSetup
	{
		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			user.PermissionInformation.SetDefaultTimeZone(CccTimeZoneInfoFactory.HawaiiTimeZoneInfo());
		}
	}
}