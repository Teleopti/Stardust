using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Client
{
	[RestoreDatabaseAfterTest]
	public class VerifyTerminalDateTest : DatabaseTest
	{
		[Test]
		public void TerminateDateSetToNull()
		{
			var person = new Person();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);
			PersistAndRemoveFromUnitOfWork(person);
			UnitOfWork.PersistAll();

			var target = new VerifyTerminalDate(() => SetupFixtureForAssembly.ApplicationData);
			target.IsTerminated(SetupFixtureForAssembly.DataSource.DataSourceName, person.Id.Value)
				.Should().Be.False();
		}

		[Test]
		public void TerminateDateSetToThePast()
		{
			var person = new Person();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);
			person.TerminatePerson(DateOnly.Today.AddDays(-5), new PersonAccountUpdaterDummy());
			PersistAndRemoveFromUnitOfWork(person);
			UnitOfWork.PersistAll();

			var target = new VerifyTerminalDate(() => SetupFixtureForAssembly.ApplicationData);
			target.IsTerminated(SetupFixtureForAssembly.DataSource.DataSourceName, person.Id.Value)
				.Should().Be.True();
		}

		[Test]
		public void TerminateDateSetToTheFuture()
		{
			var person = new Person();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);
			person.TerminatePerson(DateOnly.Today.AddDays(5), new PersonAccountUpdaterDummy());
			PersistAndRemoveFromUnitOfWork(person);
			UnitOfWork.PersistAll();

			var target = new VerifyTerminalDate(() => SetupFixtureForAssembly.ApplicationData);
			target.IsTerminated(SetupFixtureForAssembly.DataSource.DataSourceName, person.Id.Value)
				.Should().Be.False();
		}
	}
}