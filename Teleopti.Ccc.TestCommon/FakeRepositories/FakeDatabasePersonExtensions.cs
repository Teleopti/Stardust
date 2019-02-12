using System;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public static class FakeDatabasePersonExtensions
	{
		public static FakeDatabase WithPerson(this FakeDatabase database, Guid id, string name)
		{
			return database.WithPerson(id, name, null, null, null, null, null);
		}

		public static FakeDatabase WithPerson(this FakeDatabase database, string name)
		{
			return database.WithPerson(null, name, null, null, null, null, null);
		}

		public static FakeDatabase WithPerson(this FakeDatabase database, Guid id)
		{
			return database.WithPerson(id, null, null, null, null, null, null);
		}

		public static FakeDatabase WithPerson(this FakeDatabase database, Guid id, string name, TimeZoneInfo timeZone)
		{
			return database.WithPerson(id, name, null, timeZone, null, null, null);
		}
	}
}