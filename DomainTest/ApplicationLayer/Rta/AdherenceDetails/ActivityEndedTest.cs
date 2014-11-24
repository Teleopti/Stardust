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
		public void ShouldMarkActivityEnded()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = false });
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = false });
			persister.Rows.First().ActivityHasEnded.Should().Be(true);
			persister.Rows.Last().ActivityHasEnded.Should().Be(false);
		}

		[Test]
		public void ShouldMarkLastActivityEndedWhenShiftHasEnded()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = false });
			target.Handle(new PersonShiftEndEvent { PersonId = personId, ShiftStartTime = "2014-11-17 8:00".Utc()});
			persister.Rows.Last().ActivityHasEnded.Should().Be(true);
		}

		[Test]
		public void ShouldPersistInAdherenceWhenShiftHasEnded()
		{
			var personId = Guid.NewGuid();
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = true });
			target.Handle(new PersonShiftEndEvent { PersonId = personId,ShiftStartTime = "2014-11-17 8:00".Utc(), ShiftEndTime = "2014-11-17 9:00".Utc() });
			persister.Rows.First().TimeInAdherence.Should().Be(TimeSpan.FromHours(1));
		}
	}
}