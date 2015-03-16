using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.AdherenceDetails
{
	[AdherenceTest]
	[TestFixture]
	public class ShiftActualEndTimeTest
	{
		public FakeAdherenceDetailsReadModelPersister Persister;
		public AdherenceDetailsReadModelUpdater Target;

		[Test]
		public void ShouldPersistFirstOutOfAdherenceStateChangeInLastActivity()
		{
			var personId = Guid.NewGuid();
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2014-11-17 8:00".Utc(),
				InAdherence = true
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:30".Utc(),
				InAdherence = false
			});
			Target.Handle(new PersonShiftEndEvent
			{
				PersonId = personId,
				ShiftStartTime = "2014-11-17 8:00".Utc(),
				ShiftEndTime = "2014-11-17 9:00".Utc(),
			});

			Persister.Model.ActualEndTime.Should().Be("2014-11-17 8:30".Utc());
		}

		[Test]
		public void ShouldPersistFirstOutOfAdherenceStateChangeInLastActivity_WhenOnlyOutOfAdherenceStates()
		{
			var personId = Guid.NewGuid();
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2014-11-17 8:00".Utc(),
				InAdherence = true
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:30".Utc(),
				InAdherence = false
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:37".Utc(),
				InAdherence = false
			});
			Target.Handle(new PersonShiftEndEvent
			{
				PersonId = personId,
				ShiftStartTime = "2014-11-17 8:00".Utc(),
				ShiftEndTime = "2014-11-17 9:00".Utc(),
			});

			Persister.Model.ActualEndTime.Should().Be("2014-11-17 8:30".Utc());
		}

		[Test]
		public void ShouldPersistLastStateChangeThatCausedOutOfAdherence()
		{
			var personId = Guid.NewGuid();
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2014-11-17 8:00".Utc(),
				InAdherence = true
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:47".Utc(),
				InAdherence = false
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:52".Utc(),
				InAdherence = true
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:53".Utc(),
				InAdherence = false
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
				Timestamp = "2014-11-17 9:13".Utc(),
				InAdherence = false
			});

			Persister.Model.ActualEndTime.Should().Be("2014-11-17 8:53".Utc());
		}

		[Test]
		public void ShouldNotSetActualEndTimeIfActualStartNeverOccured()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2014-11-17 8:00".Utc(),
				InAdherence = true
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:00".Utc(),
				InAdherence = true
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:55".Utc(),
				InAdherence = false
			});
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2014-11-17 9:00".Utc(),
				InAdherence = false
			});
			Target.Handle(new PersonShiftEndEvent
			{
				PersonId = personId,
				ShiftStartTime = "2014-11-17 8:00".Utc(),
				ShiftEndTime = "2014-11-17 10:00".Utc(),
			});

			Persister.Model.ActualEndTime.Should().Be(null);

		}

		[Test]
		public void ShouldPersistLastStateChangeAfterShiftEnds()
		{
			var personId = Guid.NewGuid();
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 7:30".Utc(),
				InAdherence = false
			});
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2014-11-17 8:00".Utc(),
				InAdherence = true
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
				Timestamp = "2014-11-17 9:22".Utc(),
				InAdherence = false,
				InAdherenceWithPreviousActivity = true,
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 9:25".Utc(),
				InAdherence = false,
				InAdherenceWithPreviousActivity = true,
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 9:30".Utc(),
				InAdherence = false,
				InAdherenceWithPreviousActivity = false,
			});

			Persister.Model.ActualEndTime.Should().Be("2014-11-17 9:30".Utc());
		}
		
	}
}
