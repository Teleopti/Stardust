using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeLoggedOnUser : ILoggedOnUser, ILoggedOnUserIsPerson
	{
		private IPerson _person;

		public FakeLoggedOnUser(IPerson person)
		{
			_person = person;
		}

		public FakeLoggedOnUser()
		{
			_person = PersonFactory.CreatePersonWithId();
			_person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
		}
		
		public void SetFakeLoggedOnUser(IPerson person)
		{
			_person = person;
		}

		public IPerson CurrentUser()
		{
			return _person;
		}

		public string CurrentUserName()
		{
			throw new NotImplementedException();
		}

		public bool IsPerson(IPerson person)
		{
			return _person == person;
		}

		public void SetDefaultTimeZone(TimeZoneInfo timezone)
		{
			_person.PermissionInformation.SetDefaultTimeZone(timezone);
		}
	}
}