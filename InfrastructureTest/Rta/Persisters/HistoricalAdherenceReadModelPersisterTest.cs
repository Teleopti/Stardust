using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
{
	[TestFixture]
	[ReadModelUnitOfWorkTest]
	public class HistoricalAdherenceReadModelPersisterTest
	{
		public IHistoricalAdherenceReadModelPersister Target;
		public IHistoricalAdherenceReadModelReader Reader;

		[Test]
		public void ShouldPersistInAdherence()
		{
			var personId = Guid.NewGuid();
			
			Target.AddOut(personId, "2016-10-13 12:00".Utc());
			Target.AddIn(personId, "2016-10-13 12:15".Utc());

			var result = Reader.Read(personId, "2016-10-13 00:00".Utc(), "2016-10-14 00:00".Utc());
			result.OutOfAdherences.Single().StartTime.Should().Be("2016-10-13 12:00".Utc());
			result.OutOfAdherences.Single().EndTime.Should().Be("2016-10-13 12:15".Utc());
		}

		[Test]
		public void ShouldPersistNeutralAdherence()
		{
			var personId = Guid.NewGuid();

			Target.AddOut(personId, "2016-10-13 12:00".Utc());
			Target.AddNeutral(personId, "2016-10-13 12:15".Utc());

			var result = Reader.Read(personId, "2016-10-13 00:00".Utc(), "2016-10-14 00:00".Utc());
			result.OutOfAdherences.Single().StartTime.Should().Be("2016-10-13 12:00".Utc());
			result.OutOfAdherences.Single().EndTime.Should().Be("2016-10-13 12:15".Utc());
		}
		
	}
}