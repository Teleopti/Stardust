using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Specific
{
	public class UtcTimeZoneSpec
	{
	}

	public class UtcTimeZoneSetup : IUserSetup<UtcTimeZoneSpec>
	{
		public void Apply(UtcTimeZoneSpec spec, IPerson user, CultureInfo cultureInfo)
		{
			user.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
		}
	}
}