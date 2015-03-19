using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.AdherenceDetails
{
	[AdherenceTest]
	[TestFixture]
	public class LastUpdateTest
	{
		public FakeAdherenceDetailsReadModelPersister Persister;
		public AdherenceDetailsReadModelUpdater Target;

		[Test]
		public void ShouldPersistLastAdherenceTrue()
		{
			Target.Handle(new PersonStateChangedEvent
			{
				InAdherence = true
			});

			Persister.Model.LastAdherence.Value.Should().Be.True();
		}

		[Test]
		public void ShouldPersistLastAdherenceFalse()
		{
			Target.Handle(new PersonActivityStartEvent
			{
				InAdherence = false
			});

			Persister.Model.LastAdherence.Value.Should().Be.False();
		}

		[Test]
		public void ShouldPersistLastUpdateTime()
		{
			Target.Handle(new PersonStateChangedEvent
			{
				Timestamp = "2014-11-17 7:00".Utc(),
			});

			Persister.Model.LastUpdate.Should().Be("2014-11-17 7:00".Utc());
		}
	}
}