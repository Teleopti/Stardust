using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[UnitOfWorkWithLoginTest]
	public class ProjectionVersionPersisterTest
	{
		public IProjectionVersionPersister Persister;

		[Test]
		public void ShouldUpsertVersion()
		{
			var versions = Persister.LockAndGetVersions(Guid.NewGuid(), "2016-05-11".Date(), "2016-05-11".Date());

			versions.Single().Version.Should().Be(1);
		}
		
		[Test]
		public void ShouldIncrementVersion()
		{
			var person = Guid.NewGuid();
			Persister.LockAndGetVersions(person, "2016-05-11".Date(), "2016-05-11".Date());
			Persister.LockAndGetVersions(person, "2016-05-11".Date(), "2016-05-11".Date());
			var versions = Persister.LockAndGetVersions(person, "2016-05-11".Date(), "2016-05-11".Date());

			versions.Single().Version.Should().Be(3);
		}

		[Test]
		public void ShouldReturnSpecifiedDates()
		{
			Persister.LockAndGetVersions(Guid.NewGuid(), "2016-05-11".Date(), "2016-05-11".Date());

			var versions = Persister.LockAndGetVersions(Guid.NewGuid(), "2016-05-12".Date(), "2016-05-13".Date());

			versions.Select(x => x.Date).Should().Have.SameValuesAs("2016-05-12".Date(), "2016-05-13".Date());
		}

		[Test]
		public void ShouldReturnSpecifiedPerson()
		{
			var person = Guid.NewGuid();
			Persister.LockAndGetVersions(Guid.NewGuid(), "2016-05-11".Date(), "2016-05-11".Date());

			Persister.LockAndGetVersions(person, "2016-05-11".Date(), "2016-05-11".Date());
			var versions = Persister.LockAndGetVersions(person, "2016-05-11".Date(), "2016-05-11".Date());

			versions.Single().Version.Should().Be(2);
		}

		[Test]
		[SetCulture("fr-FR")]
		public void ShouldWorkInFrance()
		{
			var date = new DateOnly(new DateTime(2016, 05, 17));
			var versions = Persister.LockAndGetVersions(Guid.NewGuid(), date, date);

			versions.Single().Version.Should().Be(1);
		}
	}
}