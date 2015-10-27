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
	[AdherenceTest]
	[TestFixture]
	public class ShiftTest
	{
		public FakeAdherencePercentageReadModelPersister Persister;
		public AdherencePercentageReadModelUpdater Target;
		
		[Test]
		public void ShouldUpdateAdherenceTimesWhenShiftEnds()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2014-10-13 15:00".Utc()
			});
			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2014-10-13 16:00".Utc()
			});
			Target.Handle(new PersonShiftEndEvent
			{
				PersonId = personId,
				ShiftEndTime = "2014-10-13 17:00".Utc()
			});

			Persister.PersistedModel.TimeOutOfAdherence.Should().Be("60".Minutes());
			Persister.PersistedModel.TimeInAdherence.Should().Be("60".Minutes());
		}

		[Test]
		public void ShouldStopUpdatingAdherenceTimesWhenShiftEnded()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2014-10-13 16:00".Utc()
			});
			Target.Handle(new PersonShiftEndEvent
			{
				PersonId = personId,
				ShiftEndTime = "2014-10-13 17:00".Utc()
			});
			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2014-10-13 17:30".Utc()
			});

			Persister.PersistedModel.TimeOutOfAdherence.Should().Be(TimeSpan.Zero);
			Persister.PersistedModel.TimeInAdherence.Should().Be("60".Minutes());
		}

		[Test]
		public void ShouldStartUpdatingAdherenceTimeAfterShiftStarts()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-04-02 08:55".Utc()
			});
			Target.Handle(new PersonShiftStartEvent
			{
				PersonId = personId,
				ShiftStartTime = "2015-04-02 09:00".Utc()
			});
			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-04-02 09:05".Utc()
			});

			Persister.PersistedModel.TimeInAdherence.Should().Be("5".Minutes());
		}


		[Test]
		public void ShouldNotConsiderAdherenceOutsideOfShift()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-04-02 08:55".Utc()
			});
			Target.Handle(new PersonShiftStartEvent
			{
				PersonId = personId,
				ShiftStartTime = "2015-04-02 09:00".Utc()
			});

			Persister.PersistedModel.TimeInAdherence.Should().Be("0".Minutes());
		}
	}
}