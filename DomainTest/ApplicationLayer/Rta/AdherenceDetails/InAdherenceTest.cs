using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.AdherenceDetails
{
	[TestFixture]
	public class InAdherenceTest
	{
		[Test]
		public void ShouldPersistTimeInAdherenceWhenActivityStarts()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 7:55".Utc(), InAdherence = false });
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = true });

			persister.Details.Single().TimeInAdherence.Should().Be(TimeSpan.Zero);
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
			persister.Details.Single().TimeInAdherence.Should().Be(TimeSpan.FromHours(1));
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
			persister.Details.First().TimeInAdherence.Should().Be(TimeSpan.FromHours(1));
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
			persister.Details.First().TimeInAdherence.Should().Be(TimeSpan.FromMinutes(55));
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
			persister.Details.First().TimeInAdherence.Should().Be(TimeSpan.FromMinutes(25 + 10 + 10));
		}

		[Test]
		public void ShouldNotPersistTimeInAdherenceWhenNoActivityStarts()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 7:55".Utc(), InAdherence = true });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:55".Utc(), InAdherence = true });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 9:55".Utc(), InAdherence = true });
			
			persister.Details.Single().TimeInAdherence.Should().Be(TimeSpan.Zero);
		}
	}
}