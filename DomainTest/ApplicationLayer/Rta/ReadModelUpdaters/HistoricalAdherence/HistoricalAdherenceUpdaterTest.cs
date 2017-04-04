using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.HistoricalAdherence
{
	[DomainTest]
	[TestFixture]
	[Toggle(Toggles.RTA_SeeAllOutOfAdherencesToday_39146)]
	public class HistoricalAdherenceUpdaterTest
	{
		public FakeHistoricalAdherenceReadModelPersister Persister;
		public HistoricalAdherenceUpdater Target;
		public MutableNow Now;

		[Test]
		public void ShouldAddOutOfAdherence()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent {PersonId = personId, Timestamp = "2016-10-12 12:00".Utc()});

			Persister.Read(personId, "2016-10-12".Date()).OutOfAdherences.Single().StartTime.Should().Be("2016-10-12 12:00".Utc());
		}

		[Test]
		public void ShouldAddOutOfAdherencä()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent { PersonId = personId, Timestamp = "2016-11-12 13:00".Utc() });

			Persister.Read(personId, "2016-11-12".Date()).OutOfAdherences.Single().StartTime.Should().Be("2016-11-12 13:00".Utc());
		}
		
		[Test]
		public void ShouldPutOutOfAdherenceEndTime()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent { PersonId = personId, BelongsToDate = "2016-11-12".Date(), Timestamp = "2016-11-12 12:00".Utc() });
			Target.Handle(new PersonInAdherenceEvent { PersonId = personId, BelongsToDate = "2016-11-12".Date(), Timestamp = "2016-11-12 12:15".Utc() });

			Persister.Read(personId, "2016-11-12".Date()).OutOfAdherences.Single().EndTime.Should().Be("2016-11-12 12:15".Utc());
		}

		[Test]
		public void ShouldAddSecondOutOfAdherence()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent { PersonId = personId, BelongsToDate = "2016-11-12".Date(), Timestamp = "2016-11-12 12:00".Utc() });
			Target.Handle(new PersonInAdherenceEvent { PersonId = personId, BelongsToDate = "2016-11-12".Date(), Timestamp = "2016-11-12 12:15".Utc() });
			Target.Handle(new PersonOutOfAdherenceEvent { PersonId = personId, BelongsToDate = "2016-11-12".Date(), Timestamp = "2016-11-12 14:00".Utc() });

			Persister.Read(personId, "2016-11-12".Date()).OutOfAdherences.Should().Have.Count.EqualTo(2);
		}

		[Test]
		public void ShouldCloseOutOfAdherencesOrderedByTime()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent { PersonId = personId, BelongsToDate = "2016-11-12".Date(), Timestamp = "2016-11-12 12:00".Utc() });
			Target.Handle(new PersonOutOfAdherenceEvent { PersonId = personId, BelongsToDate = "2016-11-12".Date(), Timestamp = "2016-11-12 12:15".Utc() });

			Target.Handle(new PersonInAdherenceEvent { PersonId = personId, BelongsToDate = "2016-11-12".Date(), Timestamp = "2016-11-12 12:20".Utc() });
			Target.Handle(new PersonInAdherenceEvent { PersonId = personId, BelongsToDate = "2016-11-12".Date(), Timestamp = "2016-11-12 12:05".Utc() });

			Persister.Read(personId, "2016-11-12".Date()).OutOfAdherences.First().EndTime.Should().Be("2016-11-12 12:05".Utc());
			Persister.Read(personId, "2016-11-12".Date()).OutOfAdherences.Last().EndTime.Should().Be("2016-11-12 12:20".Utc());
		}


		[Test]
		public void ShouldEndOutOfAdherenceOnNeutral()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent { PersonId = personId, BelongsToDate = "2016-11-12".Date(), Timestamp = "2016-11-12 12:00".Utc() });
			Target.Handle(new PersonNeutralAdherenceEvent { PersonId = personId, BelongsToDate = "2016-11-12".Date(), Timestamp = "2016-11-12 12:30".Utc() });

			Persister.Read(personId, "2016-11-12".Date()).OutOfAdherences.Single().EndTime.Should().Be("2016-11-12 12:30".Utc());
		}

		[Test]
		public void ShouldEndOutOfAdherenceStartedTwoHoursBeforeToday()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent { PersonId = personId, BelongsToDate = "2016-11-11".Date(), Timestamp = "2016-11-11 22:00".Utc() });
			Target.Handle(new PersonInAdherenceEvent { PersonId = personId, BelongsToDate = "2016-11-12".Date(), Timestamp = "2016-11-12 12:00".Utc() });

			Persister.Read(personId, "2016-11-11 00:00".Utc(), "2016-11-13 00:00".Utc())
				.OutOfAdherences.Single().EndTime.Should().Be("2016-11-12 12:00".Utc());
		}

		[Test]
		public void ShouldEndOutOfAdherenceStartedMorningOfNightShift()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent { PersonId = personId, BelongsToDate = "2016-11-12".Date(), Timestamp = "2016-11-13 06:00".Utc() });

			Persister.Read(personId, "2016-11-13 00:00".Utc(), "2016-11-14 00:00".Utc())
				.OutOfAdherences.Single().StartTime.Should().Be("2016-11-13 06:00".Utc());
		}

		[Test]
		public void ShouldCloseEarliestOutOfAdherence()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent { PersonId = personId, BelongsToDate = "2016-11-12".Date(), Timestamp = "2016-11-12 06:00".Utc() });
			Target.Handle(new PersonOutOfAdherenceEvent { PersonId = personId, BelongsToDate = "2016-11-12".Date(), Timestamp = "2016-11-12 07:00".Utc() });
			Target.Handle(new PersonInAdherenceEvent { PersonId = personId, BelongsToDate = "2016-11-12".Date(), Timestamp = "2016-11-12 07:30".Utc() });

			var result = Persister.Read(personId, "2016-11-12 00:00".Utc(), "2016-11-13 00:00".Utc()).OutOfAdherences.Single();
			result.StartTime.Should().Be("2016-11-12 06:00".Utc());
			result.EndTime.Should().Be("2016-11-12 07:30".Utc());
		}

		[Test]
		public void ShouldDeleteOldData()
		{
			Now.Is("2016-10-24");
			var personId = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent { PersonId = personId, BelongsToDate = "2016-10-12".Date(), Timestamp = "2016-10-12 06:00".Utc() });
			Target.Handle(new PersonInAdherenceEvent { PersonId = personId, BelongsToDate = "2016-10-12".Date(), Timestamp = "2016-10-12 06:30".Utc() });
			Target.Handle(new PersonOutOfAdherenceEvent { PersonId = personId, BelongsToDate = "2016-10-12".Date(), Timestamp = "2016-10-12 07:00".Utc() });
			Target.Handle(new PersonInAdherenceEvent { PersonId = personId, BelongsToDate = "2016-10-12".Date(), Timestamp = "2016-10-12 07:30".Utc() });
			Target.Handle(new PersonOutOfAdherenceEvent { PersonId = personId, BelongsToDate = "2016-10-24".Date(), Timestamp = "2016-10-24 07:00".Utc() });
			Target.Handle(new PersonInAdherenceEvent { PersonId = personId, BelongsToDate = "2016-10-24".Date(), Timestamp = "2016-10-24 07:30".Utc() });

			Target.Handle(new TenantDayTickEvent());

			Persister.Read(personId).OutOfAdherences.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldKeepDataForYesterday()
		{
			Now.Is("2016-10-24");
			var personId = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent { PersonId = personId, BelongsToDate = "2016-10-22".Date(), Timestamp = "2016-10-22 06:00".Utc() });
			Target.Handle(new PersonInAdherenceEvent { PersonId = personId, BelongsToDate = "2016-10-22".Date(), Timestamp = "2016-10-22 06:30".Utc() });
			Target.Handle(new PersonOutOfAdherenceEvent { PersonId = personId, BelongsToDate = "2016-10-22".Date(), Timestamp = "2016-10-22 07:00".Utc() });
			Target.Handle(new PersonInAdherenceEvent { PersonId = personId, BelongsToDate = "2016-10-22".Date(), Timestamp = "2016-10-22 07:30".Utc() });
			Target.Handle(new PersonOutOfAdherenceEvent { PersonId = personId, BelongsToDate = "2016-10-23".Date(), Timestamp = "2016-10-23 07:00".Utc() });
			Target.Handle(new PersonInAdherenceEvent { PersonId = personId, BelongsToDate = "2016-10-23".Date(), Timestamp = "2016-10-23 07:30".Utc() });

			Target.Handle(new TenantDayTickEvent());

			var result = Persister.Read(personId).OutOfAdherences.Single();
			result.StartTime.Should().Be("2016-10-23 07:00".Utc());
		}
	}
}