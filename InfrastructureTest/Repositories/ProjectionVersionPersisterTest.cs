using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

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
			var versions = Persister.Upsert(Guid.NewGuid(), new[] {"2016-05-11".Date()});

			versions.Single().Version.Should().Be(1);
		}
		
		[Test]
		public void ShouldIncrementVersion()
		{
			var person = Guid.NewGuid();
			Persister.Upsert(person, new[] { "2016-05-11".Date() });
			Persister.Upsert(person, new[] { "2016-05-11".Date() });
			var versions = Persister.Upsert(person, new[] { "2016-05-11".Date() });

			versions.Single().Version.Should().Be(3);
		}

		[Test]
		public void ShouldReturnSpecifiedDates()
		{
			Persister.Upsert(Guid.NewGuid(), new[] {"2016-05-11".Date()});

			var versions = Persister.Upsert(Guid.NewGuid(), new[] {"2016-05-12".Date(), "2016-05-13".Date()});

			versions.Select(x => x.Date).Should().Have.SameValuesAs("2016-05-12".Date(), "2016-05-13".Date());
		}

		[Test]
		public void ShouldReturnSpecifiedPerson()
		{
			var person = Guid.NewGuid();
			Persister.Upsert(Guid.NewGuid(), new[] { "2016-05-11".Date() });

			Persister.Upsert(person, new[] { "2016-05-11".Date()});
			var versions = Persister.Upsert(person, new[] { "2016-05-11".Date()});

			versions.Single().Version.Should().Be(2);
		}
	}
}