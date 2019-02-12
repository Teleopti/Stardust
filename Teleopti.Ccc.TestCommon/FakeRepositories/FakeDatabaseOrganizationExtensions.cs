using System;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public static class FakeDatabaseOrganizationExtensions
	{
		public static FakeDatabase WithTeam(this FakeDatabase database, Guid? id)
		{
			return database.WithTeam(id, null);
		}

		public static FakeDatabase WithTeam(this FakeDatabase database, string name)
		{
			return database.WithTeam(null, name);
		}

		public static FakeDatabase WithSite(this FakeDatabase database, Guid? id)
		{
			return database.WithSite(id, "s");
		}

		public static FakeDatabase WithSite(this FakeDatabase database, string name)
		{
			return database.WithSite(null, name);
		}

		public static FakeDatabase WithSite(this FakeDatabase database)
		{
			return database.WithSite(null, "s");
		}
	}
}