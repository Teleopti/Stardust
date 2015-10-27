using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.AdherenceDetails
{
	[AdherenceTest]
	[TestFixture]
	public class ShiftEndTimeTest
	{
		public FakeAdherenceDetailsReadModelPersister Persister;
		public AdherenceDetailsReadModelUpdater Target;

		[Test]
		public void ShouldPersistShiftEndTime()
		{
			var personId = Guid.NewGuid();
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2014-11-17 8:00".Utc(),
				Adherence = EventAdherence.In
			});
			Target.Handle(new PersonShiftEndEvent
			{
				PersonId = personId,
				ShiftStartTime = "2014-11-17 8:00".Utc(),
				ShiftEndTime = "2014-11-17 9:00".Utc(),
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 9:30".Utc(),
				Adherence = EventAdherence.Out
			});

			Persister.Model.ShiftEndTime.Should().Be("2014-11-17 9:00".Utc());
		}
	}
}
