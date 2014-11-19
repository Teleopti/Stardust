using System;
using System.Linq;
using NHibernate.Criterion;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common.Time;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	[TestFixture]
	public class AdherenceDetailsReadModelUpdaterTest
	{
		#region starttimes
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
		public void ShouldPersistActualStartTimeWhenLater()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone" , InAdherence = false});

			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:05".Utc(), InAdherence = true});

			persister.Rows.Single().ActualStartTime.Should().Be("2014-11-17 8:05".Utc());
		}

		[Test]
		public void ShouldPersistActualStartTimeWhenEarlier()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone" , InAdherence = false});
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:02".Utc() ,InAdherence = true});

			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:55".Utc(), InAdherence = false});

			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 9:00".Utc(), Name = "Break", InAdherence = true});

			persister.Rows.Last().ActualStartTime.Should().Be("2014-11-17 8:55".Utc());
		}

		[Test]
		public void ShouldNotPersistActualStartTimeWhenNeverStarted()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = false});

			persister.Rows.Single().ActualStartTime.Should().Be(null);
		}

		[Test]
		public void ShouldPersistActualStartTimeWhenRedundantInAdherenceEvents()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = false});

			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:02".Utc(), InAdherence = true});

			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:10".Utc(), InAdherence = true});

			persister.Rows.Single().ActualStartTime.Should().Be("2014-11-17 8:02".Utc());
		}

		[Test]
		public void ShouldPersistActualStartTimeWhenAdherenceDoesNotChangeWhenActivityChanges()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone" , InAdherence = false});
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:02".Utc() , InAdherence = true});
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
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:02".Utc(), InAdherence = true});
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 9:00".Utc(), Name = "Lunch" , InAdherence = false});

			persister.Rows.Last().ActualStartTime.Should().Be(null);
		}

		#endregion starttimes

		#region inadherence
		[Test]
		public void ShouldPersistTimeInAdherenceWhenActivityStarts()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 7:55".Utc(), InAdherence = false });
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = true });

			persister.Rows.Single().TimeInAdherence.Should().Be(TimeSpan.Zero);
		}

		[Test]
		public void ShouldPersistTimeInAdherenceWhenStateChanges()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 7:55".Utc(), InAdherence = false });
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = true });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 9:00".Utc(), InAdherence = false });
			persister.Rows.Single().TimeInAdherence.Should().Be(TimeSpan.FromHours(1));
		}

		[Test]
		public void ShouldPersistTimeInAdherenceWhenActivityChanges()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 7:55".Utc(), InAdherence = false });
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = true });
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 9:00".Utc(), Name = "Lunch", InAdherence = false });
			persister.Rows.First().TimeInAdherence.Should().Be(TimeSpan.FromHours(1));
		}

		[Test]
		public void ShouldPersistTimeInAdherenceWhenOutAdherenceBeforeActivityChanges()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 7:55".Utc(), InAdherence = false });
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = true });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:55".Utc(), InAdherence = false });
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 9:00".Utc(), Name = "Lunch", InAdherence = false });
			persister.Rows.First().TimeInAdherence.Should().Be(TimeSpan.FromMinutes(55));
		}

		[Test]
		public void ShouldSummarizeAllInAdherenceforOneActivity()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = false });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:05".Utc(), InAdherence = true });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:30".Utc(), InAdherence = true });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:40".Utc(), InAdherence = false });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:45".Utc(), InAdherence = true });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:55".Utc(), InAdherence = false });
			persister.Rows.First().TimeInAdherence.Should().Be(TimeSpan.FromMinutes(25+10+10));
		}

		#endregion inadherence

		#region outofadherence
	

		[Test]
		public void ShouldPersistTimeOutAdherenceWhenStateChanges()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = false });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 9:00".Utc(), InAdherence = true });
			persister.Rows.Single().TimeOutAdherence.Should().Be(TimeSpan.FromHours(1));
		}

		[Test]
		public void ShouldPersistTimeOutAdherenceWhenActivityChanges()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = false });
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 9:00".Utc(), Name = "Lunch", InAdherence = false });
			persister.Rows.First().TimeOutAdherence.Should().Be(TimeSpan.FromHours(1));
		}

		[Test]
		public void ShouldPersistTimeOutOfAdherenceWhenInAdherenceBeforeActivityChanges()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:00".Utc(), InAdherence = false });
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = true });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:55".Utc(), InAdherence = false });
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 9:00".Utc(), Name = "Lunch", InAdherence = false });
			persister.Rows.First().TimeOutAdherence.Should().Be(TimeSpan.FromMinutes(5));
		}

		[Test]
		public void ShouldSummarizeAllOutOfAdherence()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = false });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:10".Utc(), InAdherence = true });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:20".Utc(), InAdherence = false });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:25".Utc(), InAdherence = true });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:45".Utc(), InAdherence = false });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:55".Utc(), InAdherence = false });
			persister.Rows.First().TimeOutAdherence.Should().Be(TimeSpan.FromMinutes(10+5+10));
		}
		#endregion outofadherence

	}
}