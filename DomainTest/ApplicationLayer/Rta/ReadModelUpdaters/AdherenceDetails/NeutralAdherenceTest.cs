using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.TestCommon;
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
		public void ShouldCalculateNeutralAdherence()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2015-03-16 8:00".Utc(),
				Adherence = AdherenceState.Neutral
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2015-03-16 9:00".Utc(),
				Adherence = AdherenceState.In
			});

			Persister.Details.Single().TimeOutOfAdherence.Should().Be(TimeSpan.Zero);
		}

		[Test]
		public void ShouldSetActualStartTime()
		{
			var personId = Guid.NewGuid();
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2014-11-17 8:00".Utc(),
				Adherence = AdherenceState.Unknown
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:00".Utc(),
				Adherence = AdherenceState.In
			});

			Persister.Details.Single().ActualStartTime.Should().Be("2014-11-17 8:00".Utc());
		}

		[Test]
		public void ShouldNotSetLastAdherenceWhenEnteringNeutralAdherence()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2015-03-16 8:00".Utc(),
				Adherence = AdherenceState.In
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2015-03-16 9:00".Utc(),
				Adherence = AdherenceState.Neutral
			});

			Persister.Model.LastAdherence.Should().Be(null);
		}

		[Test]
		public void ShouldSetActualStartTimeWhenStartedBeforeActivity()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2015-03-16 7:55".Utc(),
				Adherence = AdherenceState.Out
			});
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2015-03-16 8:00".Utc(),
				Adherence = AdherenceState.Neutral
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
				Adherence = AdherenceState.Out
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2015-03-16 8:05".Utc(),
				Adherence = AdherenceState.Neutral
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
				Adherence = AdherenceState.Neutral
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2015-03-17 8:05".Utc(),
				Adherence = AdherenceState.In
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
				Adherence = AdherenceState.In
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2015-03-17 8:05".Utc(),
				Adherence = AdherenceState.Neutral
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
				Adherence = AdherenceState.Out
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2015-03-17 8:05".Utc(),
				Adherence = AdherenceState.Neutral
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
				Adherence = AdherenceState.Out
			});
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2015-03-17 8:00".Utc(),
				Adherence = AdherenceState.Neutral
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
				Adherence = AdherenceState.Out
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2015-03-17 8:00".Utc(),
				Adherence = AdherenceState.Neutral
			});

			Persister.Details.Single().ActualStartTime.Should().Be("2015-03-17 8:00".Utc());	
		}
	}
}