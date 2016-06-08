using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.AdherencePercentage
{
	[ReadModelUpdaterTest]
	[TestFixture]
	public class NightshiftTest
	{
		public FakeAdherencePercentageReadModelPersister Persister;
		public AdherencePercentageReadModelUpdater Target;

		[Test]
		public void ShouldPersistNightshift()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonShiftStartEvent
			{
				PersonId = personId,
				BelongsToDate = "2016-06-08".Date(),
				Nightshift = true
			});

			Persister.Get("2016-06-08".Date(), personId).NightShift.Should().Be(true);
		}
	}
}