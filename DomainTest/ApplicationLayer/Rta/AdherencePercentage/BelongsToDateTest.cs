using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.AdherencePercentage
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
				BelongsToDate = "2015-02-22"
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
				BelongsToDate = "2015-02-22"
			});
			Target.Handle(new PersonInAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-02-22 23:00".Utc(),
				BelongsToDate = "2015-02-23"
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
				BelongsToDate = "2015-02-22"
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
				BelongsToDate = "2015-02-22"
			});

			Persister.Get("2015-02-22".Date(), personId).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPersistOnNeutralAdherenceBelongsToDate()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonNeutralAdherenceEvent
			{
				PersonId = personId,
				Timestamp = "2015-03-02 03:00".Utc(),
				BelongsToDate = "2015-03-01"
			});

			Persister.Get("2015-03-01".Date(), personId).Should().Not.Be.Null();
		}
	}
}