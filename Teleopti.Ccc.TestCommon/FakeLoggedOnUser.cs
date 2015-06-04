using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeLoggedOnUser : ILoggedOnUser
	{
		private IPerson _person;

		public FakeLoggedOnUser(IPerson person)
		{
			_person = person;
		}

		public void SetFakeLoggedOnUser(IPerson person)
		{
			_person = person;
		}

		public FakeLoggedOnUser()
		{
			_person = PersonFactory.CreatePersonWithId();
			_person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
		}

		public IPerson CurrentUser()
		{
			return _person;
		}

		
	}
}