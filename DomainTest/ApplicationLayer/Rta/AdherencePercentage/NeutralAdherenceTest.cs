using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.AdherencePercentage
{
	[AdherenceTest]
	[TestFixture]
	public class NeutralAdherenceTest
	{
		public FakeAdherencePercentageReadModelPersister Persister;
		public AdherencePercentageReadModelUpdater Target;

		[Test]
		public void ShouldHandleNeutralAdherenceEvent()
		{
			Target.Handle(new PersonNeutralAdherenceEvent());

			Persister.PersistedModel.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldUpdateTimeInAdherenceWhenPersonEntersNeutralAdherence()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-03-02 08:00".Utc()
			});
			Target.Handle(new PersonNeutralAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-03-02 09:00".Utc()
			});

			Persister.PersistedModel.TimeInAdherence.Should().Be("60".Minutes());
		}

		[Test]
		public void ShouldUpdateIsLastTimeInAdherenceWhenEnteringNeutralAdherence()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-03-02 08:00".Utc()
			});
			Target.Handle(new PersonNeutralAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-03-02 09:00".Utc()
			});

			Persister.PersistedModel.IsLastTimeInAdherence.Should().Be(null);
		}
	}
}