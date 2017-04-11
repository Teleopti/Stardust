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

			Persister.Read(personId, "2016-10-12".Date()).Single().Timestamp.Should().Be("2016-10-12 12:00".Utc());
		}

		[Test]
		public void ShouldAddOutOfAdherencä()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent { PersonId = personId, Timestamp = "2016-11-12 13:00".Utc() });

			Persister.Read(personId, "2016-11-12".Date()).Single().Timestamp.Should().Be("2016-11-12 13:00".Utc());
		}
		
		[Test]
		public void ShouldPutOutOfAdherenceEndTime()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent { PersonId = personId, BelongsToDate = "2016-11-12".Date(), Timestamp = "2016-11-12 12:00".Utc() });
			Target.Handle(new PersonInAdherenceEvent { PersonId = personId, BelongsToDate = "2016-11-12".Date(), Timestamp = "2016-11-12 12:15".Utc() });

			Persister.Read(personId, "2016-11-12".Date()).Last().Timestamp.Should().Be("2016-11-12 12:15".Utc());
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

			Persister.Read(personId).Should().Have.Count.EqualTo(2);
		}

		[Test]
		public void ShouldKeepDataForFiveDays()
		{
			Now.Is("2017-04-10 00:00");
			var personId = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent { PersonId = personId, BelongsToDate = "2017-04-05".Date(), Timestamp = "2017-04-05 00:00".Utc() });
			Target.Handle(new PersonInAdherenceEvent { PersonId = personId, BelongsToDate = "2017-04-09".Date(), Timestamp = "2017-04-09 12:00".Utc() });

			Target.Handle(new TenantDayTickEvent());

			var result = Persister.Read(personId);
			result.Select(x => x.Timestamp).Should().Contain("2017-04-05 00:00".Utc());
			result.Select(x => x.Timestamp).Should().Contain("2017-04-09 12:00".Utc());
		}
	}
}