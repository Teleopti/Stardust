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
	public class ActivityEndedTest
	{
		[Test]
		public void ShouldNotPersistTimeInAdherenceWhenInAdherenceBeforeShiftStarts()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 7:00".Utc(), InAdherence = true });
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = true });
			target.Handle(new PersonShiftEndEvent { PersonId = personId, ShiftStartTime = "2014-11-17 8:00".Utc(), ShiftEndTime = "2014-11-17 9:00".Utc() });
			persister.Rows.First().Model.DetailModels.First().TimeInAdherence.Should().Be(TimeSpan.FromHours(1));
		}


		[Test]
		public void ShouldNotPersistTimeInAdherenceWhenInAdherenceAfterShiftHasEnded()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = false });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 8:05".Utc(), InAdherence = true });
			target.Handle(new PersonShiftEndEvent { PersonId = personId, ShiftEndTime = "2014-11-17 9:00".Utc(), ShiftStartTime = "2014-11-17 8:00".Utc() });
			target.Handle(new PersonStateChangedEvent { PersonId = personId, Timestamp = "2014-11-17 9:05".Utc(), InAdherence = true });

			persister.Rows.First().Model.DetailModels.First().TimeInAdherence.Should().Be(TimeSpan.FromMinutes(55));
			persister.Rows.First().Model.DetailModels.First().TimeOutOfAdherence.Should().Be(TimeSpan.FromMinutes(5));
		}

		[Test]
		public void ShouldMarkLastActivityEndedWhenShiftHasEnded()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = false });
			target.Handle(new PersonShiftEndEvent { PersonId = personId, ShiftStartTime = "2014-11-17 8:00".Utc()});
			persister.Rows.First().Model.HasActivityEnded.Should().Be(true);
		}

		[Test]
		public void ShouldPersistInAdherenceWhenShiftHasEnded()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = true });
			target.Handle(new PersonShiftEndEvent { PersonId = personId,ShiftStartTime = "2014-11-17 8:00".Utc(), ShiftEndTime = "2014-11-17 9:00".Utc() });
			persister.Rows.First().Model.DetailModels.First().TimeInAdherence.Should().Be(TimeSpan.FromHours(1));
		}
	}
}