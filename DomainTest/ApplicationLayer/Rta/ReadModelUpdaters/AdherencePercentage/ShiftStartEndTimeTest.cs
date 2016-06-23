using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.AdherencePercentage
{
	[ReadModelUpdaterTest]
	[TestFixture]
	public class ShiftStartEndTimeTest
	{
		public FakeAdherencePercentageReadModelPersister Persister;
		public AdherencePercentageReadModelUpdater Target;
		
		[Test]
		public void ShouldPersistShiftStartTime()
		{
			Target.Handle(new PersonShiftStartEvent
			{
				PersonId = Guid.NewGuid(),
				ShiftStartTime =  "2016-06-23 08:00".Utc()
			});

			Persister.PersistedModel.ShiftStartTime.Should().Be("2016-06-23 08:00".Utc());
		}

		[Test]
		public void ShouldPersistShiftEndTime()
		{
			Target.Handle(new PersonShiftStartEvent
			{
				PersonId = Guid.NewGuid(),
				ShiftEndTime = "2016-06-23 17:00".Utc()
			});

			Persister.PersistedModel.ShiftEndTime.Should().Be("2016-06-23 17:00".Utc());
		}

	}
}