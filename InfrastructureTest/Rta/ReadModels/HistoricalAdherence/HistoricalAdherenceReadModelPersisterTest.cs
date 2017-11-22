using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.InfrastructureTest.Rta.ReadModels.HistoricalAdherence
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
			result.First().Timestamp.Should().Be("2016-10-13 12:00".Utc());
			result.Last().Timestamp.Should().Be("2016-10-13 12:15".Utc());
		}

		[Test]
		public void ShouldPersistNeutralAdherence()
		{
			var personId = Guid.NewGuid();

			Target.AddOut(personId, "2016-10-13 12:00".Utc());
			Target.AddNeutral(personId, "2016-10-13 12:15".Utc());

			var result = Reader.Read(personId, "2016-10-13 00:00".Utc(), "2016-10-14 00:00".Utc());
			result.First().Timestamp.Should().Be("2016-10-13 12:00".Utc());
			result.Last().Timestamp.Should().Be("2016-10-13 12:15".Utc());
		}

		[Test]
		public void ShouldDeleteOldData()
		{
			var personId = Guid.NewGuid();

			Target.AddOut(personId, "2016-10-13 12:00".Utc());
			Target.AddNeutral(personId, "2016-10-13 12:15".Utc());
			Target.AddOut(personId, "2016-10-13 13:00".Utc());
			Target.AddNeutral(personId, "2016-10-13 13:15".Utc());
			Target.AddOut(personId, "2016-10-13 14:00".Utc());
			Target.AddNeutral(personId, "2016-10-13 14:15".Utc());
			Target.AddOut(personId, "2016-10-24 09:00".Utc());
			Target.AddNeutral(personId, "2016-10-24 09:15".Utc());
			Target.AddOut(personId, "2016-10-24 12:00".Utc());
			Target.AddNeutral(personId, "2016-10-24 12:15".Utc());

			Target.Remove("2016-10-24".Utc());

			var result = Reader.Read(personId, "2016-10-01 00:00".Utc(), "2016-10-31 00:00".Utc());
			result.Should().Have.Count.EqualTo(4);
		}
	}
}