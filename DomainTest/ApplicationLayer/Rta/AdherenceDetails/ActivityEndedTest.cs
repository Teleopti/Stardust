using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.AdherenceDetails
{
	[AdherenceTest]
	[TestFixture]
	public class ActivityEndedTest
	{
		public FakeAdherenceDetailsReadModelPersister Persister;
		public AdherenceDetailsReadModelUpdater Target;

		[Test]
		public void ShouldNotPersistTimeInOfAdherenceWhenInAdherenceBeforeShiftStarts()
		{
			var personId = Guid.NewGuid();
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 7:00".Utc(),
				InAdherence = true
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
				ShiftEndTime = "2014-11-17 9:00".Utc()
			});

			Persister.Details.First().TimeInAdherence.Should().Be(TimeSpan.FromHours(1));
		}

		[Test]
		public void ShouldNotPersistTimeInAdherenceWhenInAdherenceAfterShiftHasEnded()
		{
			var personId = Guid.NewGuid();
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2014-11-17 8:00".Utc(),
				InAdherence = false
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:05".Utc(),
				InAdherence = true
			});
			Target.Handle(new PersonShiftEndEvent
			{
				PersonId = personId,
				ShiftEndTime = "2014-11-17 9:00".Utc(),
				ShiftStartTime = "2014-11-17 8:00".Utc()
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 9:05".Utc(),
				InAdherence = true
			});

			Persister.Details.First().TimeInAdherence.Should().Be(TimeSpan.FromMinutes(55));
			Persister.Details.First().TimeOutOfAdherence.Should().Be(TimeSpan.FromMinutes(5));
		}

		[Test]
		public void ShouldPersistInAdherenceWhenShiftHasEnded()
		{
			var personId = Guid.NewGuid();
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
				ShiftEndTime = "2014-11-17 9:00".Utc()
			});

			Persister.Details.First().TimeInAdherence.Should().Be(TimeSpan.FromHours(1));
		}
	}
}