using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common.Time;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.AdherenceDetails
{
	[TestFixture]
	public class StartTimesTest
	{
		[Test]
		public void ShouldPersist()
		{
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonActivityStartEvent
			{
				PersonId = Guid.NewGuid(),
				Name = "Phone",
				StartTime = "2014-11-17 8:00".Utc()
			});

			persister.Rows.Single().Name.Should().Be("Phone");
		}

		[Test]
		public void ShouldPersistEachActivityStarted()
		{
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonActivityStartEvent
			{
				PersonId = Guid.NewGuid(),
				Name = "Phone",
				StartTime = "2014-11-17 8:00".Utc()
			});
			target.Handle(new PersonActivityStartEvent
			{
				PersonId = Guid.NewGuid(),
				Name = "Break",
				StartTime = "2014-11-17 9:00".Utc()
			});

			persister.Rows.Select(x => x.Name).Should().Have.SameValuesAs("Phone", "Break");
		}

		[Test]
		public void ShouldPersistActivitiesStartTime()
		{
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonActivityStartEvent
			{
				PersonId = Guid.NewGuid(),
				Name = "Phone",
				StartTime = "2014-11-17 8:00".Utc()
			});

			persister.Rows.Single().StartTime.Should().Be("2014-11-17 8:00".Utc());
		}

		[Test]
		public void ShouldPersistActivitiesStartTimeWhenStateChangedBeforeShiftStarted()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 7:55".Utc(),
				InAdherence = false
			});
			target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				Name = "Phone",
				StartTime = "2014-11-17 8:00".Utc(),
				InAdherence = true
			});

			persister.Rows.Single().ActualStartTime.Should().Be("2014-11-17 7:55".Utc());
		}

		[Test]
		public void ShouldPersistActualStartTime()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				Name = "Phone",
				StartTime = "2014-11-17 8:00".Utc()
			});
			target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:00".Utc(),
				InAdherence = true
			});
			persister.Rows.Single().ActualStartTime.Should().Be("2014-11-17 8:00".Utc());
		}

		[Test]
		public void ShouldPersistActualStartTimeWhenStartedLater()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = false });

			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:05".Utc(), InAdherence = true });

			persister.Rows.Single().ActualStartTime.Should().Be("2014-11-17 8:05".Utc());
		}

		[Test]
		public void ShouldPersistActualStartTimeWhenStartedEarlier()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = false });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:02".Utc(), InAdherence = true });

			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:55".Utc(), InAdherence = false });

			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 9:00".Utc(), Name = "Break", InAdherence = true });

			persister.Rows.Last().ActualStartTime.Should().Be("2014-11-17 8:55".Utc());
		}

		[Test]
		public void ShouldNotPersistActualStartTimeWhenNeverStarted()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = false });

			persister.Rows.Single().ActualStartTime.Should().Be(null);
		}

		[Test]
		public void ShouldPersistActualStartTimeWhenRedundantInAdherenceEvents()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = false });

			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:02".Utc(), InAdherence = true });

			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:10".Utc(), InAdherence = true });

			persister.Rows.Single().ActualStartTime.Should().Be("2014-11-17 8:02".Utc());
		}

		[Test]
		public void ShouldPersistActualStartTimeWhenAdherenceDoesNotChangeWhenActivityChanges()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = false });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:02".Utc(), InAdherence = true });
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 9:00".Utc(), Name = "Phone 2", InAdherence = true });

			persister.Rows.Last().ActualStartTime.Should().Be("2014-11-17 9:00".Utc());
		}

		[Test]
		public void ShouldNotPersistActualStartTimeWhenSecondActivityNeverStarted()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = false });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:02".Utc(), InAdherence = true });
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 9:00".Utc(), Name = "Lunch", InAdherence = false });

			persister.Rows.Last().ActualStartTime.Should().Be(null);
		}
	}
}