using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.AdherenceDetails
{
	[ReadModelUpdaterTest]
	[TestFixture]
	public class BelongsToDateTest
	{
		public FakeAdherenceDetailsReadModelPersister Persister;
		public AdherenceDetailsReadModelUpdater Target;

		[Test]
		public void ShouldPersistOnActivityStartBelongsToDate()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2015-02-23 3:00".Utc(),
				BelongsToDate = new DateOnly(2015,02,22)
			});

			Persister.Get(personId, "2015-02-22".Date()).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPersistOnStateChangedBelongsToDate()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp= "2015-02-23 3:00".Utc(),
				BelongsToDate = new DateOnly(2015,02,22)
			});

			Persister.Get(personId, "2015-02-22".Date()).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPersistOnShiftEndBelongsToDate()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonShiftEndEvent
			{
				PersonId = personId,
				ShiftEndTime = "2015-02-23 3:00".Utc(),
				BelongsToDate = new DateOnly(2015,02,22)
			});

			Persister.Get(personId, "2015-02-22".Date()).Should().Not.Be.Null();
		}

	}
}