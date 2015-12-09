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
	[ReadModelUpdaterTest]
	[TestFixture]
	public class OutOfAdherenceTest
	{
		public FakeAdherenceDetailsReadModelPersister Persister;
		public AdherenceDetailsReadModelUpdater Target;

		[Test]
		public void ShouldPersistTimeOutAdherenceWhenStateChanges()
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
				Timestamp = "2014-11-17 9:00".Utc(),
				Adherence = EventAdherence.In
			});

			Persister.Details.Single().TimeOutOfAdherence.Should().Be(TimeSpan.FromHours(1));
		}

		[Test]
		public void ShouldPersistTimeOutAdherenceWhenActivityChanges()
		{
			var personId = Guid.NewGuid();
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2014-11-17 8:00".Utc(),
				Adherence = EventAdherence.Out
			});
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2014-11-17 9:00".Utc(),
				Adherence = EventAdherence.Out
			});

			Persister.Details.First().TimeOutOfAdherence.Should().Be(TimeSpan.FromHours(1));
		}

		[Test]
		public void ShouldPersistTimeOutOfAdherenceWhenInAdherenceBeforeActivityChanges()
		{
			var personId = Guid.NewGuid();
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:00".Utc(),
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

			Persister.Details.First().TimeOutOfAdherence.Should().Be(TimeSpan.FromMinutes(5));
		}

		[Test]
		public void ShouldSummarizeAllOutOfAdherence()
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
				Timestamp = "2014-11-17 8:10".Utc(),
				Adherence = EventAdherence.In
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:20".Utc(),
				Adherence = EventAdherence.Out
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:25".Utc(),
				Adherence = EventAdherence.In
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:45".Utc(),
				Adherence = EventAdherence.Out
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:55".Utc(),
				Adherence = EventAdherence.Out
			});

			Persister.Details.First().TimeOutOfAdherence.Should().Be(TimeSpan.FromMinutes(10 + 5 + 10));
		}
	}
}