using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.AdherencePercentage
{
	[ReadModelUpdaterTest]
	[TestFixture]
	public class NeutralAdherenceTest
	{
		public FakeAdherencePercentageReadModelPersister Persister;
		public AdherencePercentageReadModelUpdater Target;

		[Test]
		public void ShouldHandleNeutralAdherenceEvent()
		{
			var personId = Guid.NewGuid();
			Target.Handle(new PersonNeutralAdherenceEvent
			{
				PersonId = personId
			});

			Persister.PersistedModel.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldNotConsiderTimeInNeutralAdherence()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonNeutralAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2016-03-16 08:00".Utc()
			});
			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2016-03-16 09:00".Utc()
			});
			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2016-03-16 10:00".Utc()
			});

			Persister.PersistedModel.TimeInAdherence.Should().Be("60".Minutes());
		}

		[Test]
		public void ShouldNotSetIsLastTimeInAdherenceWhenInNeutralAdherence()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2016-03-16 08:00".Utc()
			});
			Target.Handle(new PersonNeutralAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2016-03-16 09:00".Utc()
			});

			Persister.PersistedModel.IsLastTimeInAdherence.Should().Be(null);
		}

	}
}