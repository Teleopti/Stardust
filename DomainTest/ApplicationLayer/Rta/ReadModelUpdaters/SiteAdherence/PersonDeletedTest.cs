using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.SiteAdherence
{
	[ReadModelUpdaterTest]
	[TestFixture]
	[Toggle(Toggles.RTA_DeletedPersons_36041)]
	public class PersonDeletedTest
	{
		public FakeSiteOutOfAdherenceReadModelPersister Persister;
		public SiteOutOfAdherenceReadModelUpdater Target;

		[Test]
		public void ShouldExludeDeletedPerson()
		{
			var siteId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId, PersonId = personId });

			Target.Handle(new PersonDeletedEvent
			{
				PersonId = personId,
				PersonPeriodsBefore = new[] {new PersonPeriodDetail {SiteId = siteId}}
			});

			Persister.Get(siteId).Count.Should().Be(0);
		}
		
		[Test]
		public void ShouldExludeFromAllPastSites()
		{
			var siteId1 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId1, PersonId = personId });
			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId2, PersonId = personId });

			Target.Handle(new PersonDeletedEvent
			{
				PersonId = personId,
				PersonPeriodsBefore = new[] {new PersonPeriodDetail {SiteId = siteId1}, new PersonPeriodDetail {SiteId = siteId2}}
			});

			Persister.Get(siteId1).Count.Should().Be(0);
			Persister.Get(siteId2).Count.Should().Be(0);
		}

		[Test]
		public void ShouldExludeDeletedPersonNextUpdate()
		{
			var siteId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId, PersonId = personId });

			Target.Handle(new PersonDeletedEvent
			{
				PersonId = personId,
				PersonPeriodsBefore = new[] { new PersonPeriodDetail { SiteId = siteId } }
			});
			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId, PersonId = Guid.NewGuid() });

			Persister.Get(siteId).Count.Should().Be(1);
		}

		[Test]
		public void ShouldIgnoreAdherenceChangesForDeletedPersonsForAWhile()
		{
			var siteId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Target.Handle(new PersonOutOfAdherenceEvent {SiteId = siteId, PersonId = personId, Timestamp = "2015-12-04 08:00".Utc()});

			Target.Handle(new PersonDeletedEvent
			{
				PersonId = personId,
				PersonPeriodsBefore = new[] { new PersonPeriodDetail { SiteId = siteId } },
				Timestamp = "2015-12-04 10:00".Utc()
			});
			Target.Handle(new PersonOutOfAdherenceEvent {SiteId = siteId, PersonId = personId, Timestamp = "2015-12-04 10:05".Utc()});

			Persister.Get(siteId).Count.Should().Be(0);
		}
	}
}