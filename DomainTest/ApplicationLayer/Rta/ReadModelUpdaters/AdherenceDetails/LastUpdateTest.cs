using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.AdherenceDetails
{
	[ReadModelUpdaterTest]
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
				Adherence = EventAdherence.In
			});

			Persister.Model.LastAdherence.Value.Should().Be.True();
		}

		[Test]
		public void ShouldPersistLastAdherenceFalse()
		{
			Target.Handle(new PersonActivityStartEvent
			{
				Adherence = EventAdherence.Out
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