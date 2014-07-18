using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeLoggedOnUser : ILoggedOnUser
	{
		private readonly IPerson _person;

		public FakeLoggedOnUser(IPerson person)
		{
			_person = person;
		}

		public FakeLoggedOnUser()
		{
			_person = new Person();
			_person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
		}

		public IPerson CurrentUser()
		{
			return _person;
		}
	}
}