using System;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public static class FakeDatabaseAgentExtensions
	{
		public static FakeDatabase WithAgent(this FakeDatabase database)
		{
			return database.WithAgent(null, RandomName.Make(), null, null, null, null, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, Guid personId)
		{
			return database.WithAgent(personId, RandomName.Make(), null, null, null, null, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, string name)
		{
			return database.WithAgent(null, name, null, null, null, null, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, Guid? id, string name)
		{
			return database.WithAgent(id, name, null, null, null, null, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, string name, Guid id)
		{
			return database.WithAgent(id, name, null, null, null, null, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, Guid? personId, string name, Guid? teamId, Guid? siteId)
		{
			return database.WithAgent(personId, name, null, teamId, siteId, null, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, Guid? personId, string name, Guid? teamId, Guid? siteId, Guid? businessUnitId)
		{
			return database.WithAgent(personId, name, null, teamId, siteId, businessUnitId, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, Guid? personId, string name, Guid? teamId, Guid? siteId, Guid? businessUnitId, int? employmentNumber)
		{
			return database.WithAgent(personId, name, null, teamId, siteId, businessUnitId, null, employmentNumber);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, string name, Guid personId, Guid? businessUnitId, Guid? teamId, Guid? siteId)
		{
			return database.WithAgent(personId, name, null, teamId, siteId, businessUnitId, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, string name, Guid? teamId)
		{
			return database.WithAgent(null, name, null, teamId, null, null, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, string name, Guid? teamId, Guid? siteId)
		{
			return database.WithAgent(null, name, null, teamId, siteId, null, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, string name, Guid? teamId, Guid? siteId, Guid? businessUnitId)
		{
			return database.WithAgent(null, name, null, teamId, siteId, businessUnitId, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, string name, string terminalDate)
		{
			return database.WithAgent(null, name, terminalDate, null, null, null, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, string name, string terminalDate, Guid? teamId)
		{
			return database.WithAgent(null, name, terminalDate, teamId, null, null, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, Guid? id, string name, string terminalDate, Guid? teamId)
		{
			return database.WithAgent(id, name, terminalDate, teamId, null, null, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, Guid? id, string name, int? employeeNumber)
		{
			return database.WithAgent(id, name, null, null, null, null, null, employeeNumber);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, Guid? id, string name, string terminalDate)
		{
			return database.WithAgent(id, name, terminalDate, null, null, null, null, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, string name, TimeZoneInfo timeZone)
		{
			return database.WithAgent(null, name, null, null, null, null, timeZone, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, string name, string terminalDate, TimeZoneInfo timeZone)
		{
			return database.WithAgent(null, name, terminalDate, null, null, null, timeZone, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, Guid? id, string name, TimeZoneInfo timeZone)
		{
			return database.WithAgent(id, name, null, null, null, null, timeZone, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, Guid? id, TimeZoneInfo timeZone)
		{
			return database.WithAgent(id, null, null, null, null, null, timeZone, null);
		}

		public static FakeDatabase WithAgent(this FakeDatabase database, Guid? id, string name, string terminalDate, Guid? teamId, Guid? siteId, Guid? businessUnitId, TimeZoneInfo timeZone, int? employeeNumber)
		{
			database.WithPerson(id, name, terminalDate, timeZone, null, null, employeeNumber);
			database.WithPeriod(null, teamId, siteId, businessUnitId);
			database.WithExternalLogon(name);
			database.WithSchedulePeriod(null);
			return database;
		}
	}
}