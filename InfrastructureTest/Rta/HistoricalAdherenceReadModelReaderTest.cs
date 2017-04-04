using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[DatabaseTest]
	[Toggle(Toggles.RTA_SeeAllOutOfAdherencesToday_39146)]
	public class HistoricalAdherenceReadModelReaderTest
	{
		public Database Database;
		public IHistoricalAdherenceReadModelPersister Persister;
		public IHistoricalAdherenceReadModelReader Reader;
		public IPersonRepository Persons;
		public WithUnitOfWork WithUnitOfWork;
		public WithReadModelUnitOfWork WithReadModelUnitOfWork;

		[Test]
		public void ShouldReadOutOfAdherences()
		{
			var person = Guid.NewGuid();
			WithReadModelUnitOfWork.Do(() =>
			{
				Persister.AddOut(person, "2016-10-18 08:00".Utc());
				Persister.AddIn(person, "2016-10-18 08:05".Utc());
				Persister.AddOut(person, "2016-10-18 09:00".Utc());
				Persister.AddNeutral(person, "2016-10-18 09:15".Utc());
			});

			var result = WithReadModelUnitOfWork.Get(() => Reader.Read(person, "2016-10-18 00:00".Utc(), "2016-10-19 00:00".Utc()));

			result.OutOfAdherences.First().StartTime.Should().Be("2016-10-18 08:00".Utc());
			result.OutOfAdherences.First().EndTime.Should().Be("2016-10-18 08:05".Utc());
			result.OutOfAdherences.Last().StartTime.Should().Be("2016-10-18 09:00".Utc());
			result.OutOfAdherences.Last().EndTime.Should().Be("2016-10-18 09:15".Utc());
		}
		
	}
}