using System.Globalization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups
{
	public class HawaiiTimeZone : IUserSetup
	{
		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			user.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.HawaiiTimeZoneInfo());
		}
	}

	//henke, refact out.....
	public class UserTimeZone : IUserSetup
	{
		private readonly string _timeZone;

		public UserTimeZone(string timeZone)
		{
			_timeZone = timeZone;
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			user.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.TimeZone(_timeZone));

		}
	}

}