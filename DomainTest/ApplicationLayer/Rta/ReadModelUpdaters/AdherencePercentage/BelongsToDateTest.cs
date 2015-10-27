using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.AdherencePercentage
{
	[AdherenceTest]
	[TestFixture]
	public class BelongsToDateTest
	{
		public FakeAdherencePercentageReadModelPersister Persister;
		public AdherencePercentageReadModelUpdater Target;

		[Test]
		public void ShouldPersistOnInAdherenceBelongsToDate()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-02-23 3:00".Utc(),
				BelongsToDate = new DateOnly(2015,02,22)
			});

			Persister.Get("2015-02-22".Date(), personId).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldUpdateOnInAdherenceBelongsToDate()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-02-22 12:00".Utc(),
				BelongsToDate = new DateOnly(2015,02,22)
			});
			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-02-22 23:00".Utc(),
				BelongsToDate = new DateOnly(2015,02,23)
			});

			Persister.Get("2015-02-23".Date(), personId).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPersistOnOutOfAdherenceBelongsToDate()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-02-23 3:00".Utc(),
				BelongsToDate = new DateOnly(2015,02,22)
			});

			Persister.Get("2015-02-22".Date(), personId).Should().Not.Be.Null();
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

			Persister.Get("2015-02-22".Date(), personId).Should().Not.Be.Null();
		}

	}
}