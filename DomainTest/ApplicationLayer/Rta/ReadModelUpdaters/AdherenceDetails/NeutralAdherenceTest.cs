using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.AdherenceDetails
{
	[AdherenceTest]
	[TestFixture]
	[Toggle(Toggles.RTA_NeutralAdherence_30930)]
	public class NeutralAdherenceTest
	{
		public FakeAdherenceDetailsReadModelPersister Persister;
		public AdherenceDetailsReadModelUpdater Target;

		[Test]
		public void ShouldNotSetTimeInAdherenceWhenOnlyInNeutral()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2015-03-17 8:00".Utc(),
				Adherence = EventAdherence.Neutral
			});
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2015-03-17 9:00".Utc(),
				Adherence = EventAdherence.In
			});

			Persister.Details.First().TimeInAdherence.Should().Be(null);
		}

		[Test]
		public void ShouldNotSetTimeOutOfAdherenceWhenOnlyInNeutral()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2015-03-17 8:00".Utc(),
				Adherence = EventAdherence.Neutral
			});
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2015-03-17 9:00".Utc(),
				Adherence = EventAdherence.In
			});

			Persister.Details.First().TimeOutOfAdherence.Should().Be(null);
		}

		[Test]
		public void ShouldNotSetLastAdherenceWhenEnteringNeutralAdherence()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2015-03-16 8:00".Utc(),
				Adherence = EventAdherence.In
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2015-03-16 9:00".Utc(),
				Adherence = EventAdherence.Neutral
			});

			Persister.Model.LastAdherence.Should().Be(null);
		}

		[Test]
		public void ShouldSetActualStartTime()
		{
			var personId = Guid.NewGuid();
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2014-11-17 8:00".Utc(),
				Adherence = EventAdherence.Neutral
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:00".Utc(),
				Adherence = EventAdherence.In
			});

			Persister.Details.Single().ActualStartTime.Should().Be("2014-11-17 8:00".Utc());
		}

		[Test]
		public void ShouldSetActualStartTimeWhenStartedBeforeActivity()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2015-03-16 7:55".Utc(),
				Adherence = EventAdherence.Out
			});
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2015-03-16 8:00".Utc(),
				Adherence = EventAdherence.Neutral
			});

			Persister.Details.Single().ActualStartTime.Should().Be("2015-03-16 7:55".Utc());
		}

		[Test]
		public void ShouldSetActualStartTimeWhenStartedAfterActivity()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2015-03-16 8:00".Utc(),
				Adherence = EventAdherence.Out
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2015-03-16 8:05".Utc(),
				Adherence = EventAdherence.Neutral
			});

			Persister.Details.Single().ActualStartTime.Should().Be("2015-03-16 8:05".Utc());
		}


		[Test]
		public void ShouldSetActualStartWhenTransitionFromNeutralToInAdherence()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2015-03-17 8:00".Utc(),
				Adherence = EventAdherence.Neutral
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2015-03-17 8:05".Utc(),
				Adherence = EventAdherence.In
			});

			Persister.Details.Single().ActualStartTime.Should().Be("2015-03-17 8:05".Utc());
		}


		[Test]
		public void ShouldSetActualStartWhenTransitionFromInToNeutralAdherence()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2015-03-17 8:00".Utc(),
				Adherence = EventAdherence.In
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2015-03-17 8:05".Utc(),
				Adherence = EventAdherence.Neutral
			});

			Persister.Details.Single().ActualStartTime.Should().Be("2015-03-17 8:05".Utc());
		}

		[Test]
		public void ShouldSetActualStartToStateChangeWhenTransitionFromOutToNeutralAdherence()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2015-03-17 8:00".Utc(),
				Adherence = EventAdherence.Out
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2015-03-17 8:05".Utc(),
				Adherence = EventAdherence.Neutral
			});

			Persister.Details.Single().ActualStartTime.Should().Be("2015-03-17 8:05".Utc());
		}

		[Test]
		public void ShouldSetActualStartToActivityChangeWhenTransitionFromOutToNeutralAdherence()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2015-03-17 7:55".Utc(),
				Adherence = EventAdherence.Out
			});
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2015-03-17 8:00".Utc(),
				Adherence = EventAdherence.Neutral
			});

			Persister.Details.Single().ActualStartTime.Should().Be("2015-03-17 7:55".Utc());
		}

		[Test]
		public void ShouldSetActualStartTimeWhenTransitionOccursAtTheSameTime()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2015-03-17 8:00".Utc(),
				Adherence = EventAdherence.Out
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2015-03-17 8:00".Utc(),
				Adherence = EventAdherence.Neutral
			});

			Persister.Details.Single().ActualStartTime.Should().Be("2015-03-17 8:00".Utc());	
		}


		[Test]
		public void ShouldPersistActualStartTimeWhenAdherenceDoesNotChangeWhenActivityChanges()
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
				Timestamp = "2014-11-17 8:02".Utc(),
				Adherence = EventAdherence.Neutral
			});
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2014-11-17 9:00".Utc(),
				Adherence = EventAdherence.Neutral
			});

			Persister.Details.Last().ActualStartTime.Should().Be("2014-11-17 9:00".Utc());
		}

		[Test]
		public void ShouldSetActualEndTimeWhenShiftEnds()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2015-03-17 8:00".Utc(),
				Adherence = EventAdherence.In
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2015-03-17 8:00".Utc(),
				Adherence = EventAdherence.In
			});
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2015-03-17 9:00".Utc(),
				Adherence = EventAdherence.Neutral
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2015-03-17 9:00".Utc(),
				Adherence = EventAdherence.Neutral
			});
			Target.Handle(new PersonShiftEndEvent
			{
				PersonId = personId,
				ShiftStartTime= "2015-03-17 8:00".Utc(),
				ShiftEndTime = "2015-03-17 10:00".Utc()
			});

			Persister.Details.Last().ActualStartTime.Should().Be("2015-03-17 9:00".Utc());	
		}


		[Test]
		public void ShouldSetActualStartTimeWhenTransitionFromNeutralToIn()
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
				Timestamp = "2014-11-17 8:02".Utc(),
				Adherence = EventAdherence.Neutral
			});
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2014-11-17 9:00".Utc(),
				Adherence = EventAdherence.In
			});

			Persister.Details.Last().ActualStartTime.Should().Be("2014-11-17 9:00".Utc());
		}
		
		[Test]
		public void ShouldSetActualStartTimeWhenNoStateChange()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2015-04-07 10:00".Utc(),
				Adherence = EventAdherence.Out
			});
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2015-04-07 10:15".Utc(),
				Adherence = EventAdherence.Neutral
			});
			Persister.Details.First().ActualStartTime.Should().Be(null);
			Persister.Details.Second().ActualStartTime.Should().Be("2015-04-07 10:15".Utc());
		}

		[Test]
		public void ShouldNotSetTimeInAdherenceWhenOnlyInUnknown()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2015-03-17 8:00".Utc(),
				Adherence = EventAdherence.Neutral
			});
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2015-03-17 9:00".Utc(),
				Adherence = EventAdherence.In
			});

			Persister.Details.First().TimeInAdherence.Should().Be(null);
		}
	}
}