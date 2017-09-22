using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.HistoricalAdherence
{
	[DomainTest]
	[TestFixture]
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
		
	}
}