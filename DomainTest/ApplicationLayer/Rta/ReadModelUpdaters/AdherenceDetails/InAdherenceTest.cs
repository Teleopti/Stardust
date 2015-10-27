using System;
using System.Linq;
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
	public class InAdherenceTest
	{
		public FakeAdherenceDetailsReadModelPersister Persister;
		public AdherenceDetailsReadModelUpdater Target;

		[Test]
		public void ShouldPersistTimeInAdherenceWhenActivityStarts()
		{
			var personId = Guid.NewGuid();
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 7:55".Utc(),
				Adherence = EventAdherence.Out
			});
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2014-11-17 8:00".Utc(),
				Adherence = EventAdherence.In
			});

			Persister.Details.Single().TimeInAdherence.Should().Be(TimeSpan.Zero);
		}

		[Test]
		public void ShouldPersistTimeInAdherenceWhenStateChanges()
		{
			var personId = Guid.NewGuid();
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 7:55".Utc(),
				Adherence = EventAdherence.Out
			});
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2014-11-17 8:00".Utc(),
				Adherence = EventAdherence.In
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 9:00".Utc(),
				Adherence = EventAdherence.Out
			});

			Persister.Details.Single().TimeInAdherence.Should().Be(TimeSpan.FromHours(1));
		}

		[Test]
		public void ShouldPersistTimeInAdherenceWhenActivityChanges()
		{
			var personId = Guid.NewGuid();
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 7:55".Utc(),
				Adherence = EventAdherence.Out
			});
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2014-11-17 8:00".Utc(),
				Adherence = EventAdherence.In
			});
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2014-11-17 9:00".Utc(),
				Adherence = EventAdherence.Out
			});

			Persister.Details.First().TimeInAdherence.Should().Be(TimeSpan.FromHours(1));
		}

		[Test]
		public void ShouldPersistTimeInAdherenceWhenOutAdherenceBeforeActivityChanges()
		{
			var personId = Guid.NewGuid();
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 7:55".Utc(),
				Adherence = EventAdherence.Out
			});
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2014-11-17 8:00".Utc(),
				Adherence = EventAdherence.In
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:55".Utc(),
				Adherence = EventAdherence.Out
			});
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2014-11-17 9:00".Utc(),
				Adherence = EventAdherence.Out
			});

			Persister.Details.First().TimeInAdherence.Should().Be(TimeSpan.FromMinutes(55));
		}

		[Test]
		public void ShouldSummarizeAllInAdherenceforOneActivity()
		{
			var personId = Guid.NewGuid();
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2014-11-17 8:00".Utc(),
				Adherence = EventAdherence.Out
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:05".Utc(),
				Adherence = EventAdherence.In
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:30".Utc(),
				Adherence = EventAdherence.In
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:40".Utc(),
				Adherence = EventAdherence.Out
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:45".Utc(),
				Adherence = EventAdherence.In
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:55".Utc(),
				Adherence = EventAdherence.Out
			});

			Persister.Details.First().TimeInAdherence.Should().Be(TimeSpan.FromMinutes(25 + 10 + 10));
		}

		[Test]
		public void ShouldNotPersistTimeInAdherenceWhenNoActivityStarts()
		{
			var personId = Guid.NewGuid();
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 7:55".Utc(),
				Adherence = EventAdherence.In
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:55".Utc(),
				Adherence = EventAdherence.In
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 9:55".Utc(),
				Adherence = EventAdherence.In
			});

			Persister.Details.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotPersistTimeInOfAdherenceWhenInAdherenceBeforeShiftStarts()
		{
			var personId = Guid.NewGuid();
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 7:00".Utc(),
				Adherence = EventAdherence.In
			});
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
				Adherence = EventAdherence.Out
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:05".Utc(),
				Adherence = EventAdherence.In
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
				Adherence = EventAdherence.In
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
				Adherence = EventAdherence.In
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