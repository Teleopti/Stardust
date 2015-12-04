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
	[AdherenceTest]
	[TestFixture]
	[Toggle(Toggles.RTA_DeletedPersons_36041)]
	public class PersonDeletedTest
	{
		public FakeSiteOutOfAdherenceReadModelPersister Persister;
		public SiteOutOfAdherenceReadModelUpdater Target;

		[Test]
		public void ShouldRecalculateForPersonsSite()
		{
			var siteId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId, PersonId = personId });

			Target.Handle(new PersonDeletedEvent { PersonId = personId });

			Persister.Get(siteId).Count.Should().Be(0);
		}

		[Test]
		public void ShouldNotRecalculateOtherSite()
		{
			var siteId1 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();
			var personId = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId1, PersonId = Guid.NewGuid() });
			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId2, PersonId = personId });

			Target.Handle(new PersonDeletedEvent { PersonId = personId });

			Persister.Get(siteId1).Count.Should().Be(1);
		}

		[Test]
		public void ShouldRecalculateForAllPersonsSites()
		{
			var siteId1 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId1, PersonId = personId });
			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId2, PersonId = personId });

			Target.Handle(new PersonDeletedEvent { PersonId = personId });

			Persister.Get(siteId1).Count.Should().Be(0);
			Persister.Get(siteId2).Count.Should().Be(0);
		}

		[Test]
		public void ShouldNotIncludeDeletedPersonInNextRecalculation()
		{
			var site = Guid.NewGuid();
			var person = Guid.NewGuid();
			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = site, PersonId = person });

			Target.Handle(new PersonDeletedEvent { PersonId = person });
			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = site, PersonId = new Guid() });

			Persister.Get(site).Count.Should().Be(1);
		}

		[Test]
		public void ShouldRememberPersonWasDeletedForACoupleOfMinutes()
		{
			var site = Guid.NewGuid();
			var person = Guid.NewGuid();
			Target.Handle(new PersonOutOfAdherenceEvent {SiteId = site, PersonId = person, Timestamp = "2015-12-04 08:00".Utc()});

			Target.Handle(new PersonDeletedEvent {PersonId = person, Timestamp = "2015-12-04 10:00".Utc()});
			Target.Handle(new PersonOutOfAdherenceEvent {SiteId = site, PersonId = person, Timestamp = "2015-12-04 10:05".Utc()});

			Persister.Get(site).Count.Should().Be(0);
		}
	}
}