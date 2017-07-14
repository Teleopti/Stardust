using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Specific
{
	public class SpecificUserTimeZone : IUserSetup
	{
		private readonly string _timeZoneId;

		public SpecificUserTimeZone(string timeZoneId)
		{
			_timeZoneId = timeZoneId;
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			user.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById(_timeZoneId));
		}
	}
}