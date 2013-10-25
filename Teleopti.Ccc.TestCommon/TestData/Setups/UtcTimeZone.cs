using System.Globalization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups
{
	public class UtcTimeZone : IUserSetup
	{
		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			user.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
		}
	}
}