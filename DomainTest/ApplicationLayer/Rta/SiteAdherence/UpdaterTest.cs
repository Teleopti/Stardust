using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.SiteAdherence
{
	[TestFixture]
	public class UpdaterTest
	{
		[Test]
		public void ShouldPersistSiteAdherenceOnInAdherence()
		{
			var siteId = Guid.NewGuid();
			var persister = new FakeSiteOutOfAdherenceReadModelPersister();
			var target = new SiteOutOfAdherenceReadModelUpdater(persister);

			target.Handle(new PersonInAdherenceEvent() { SiteId = siteId });

			persister.Get(siteId).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPersistSiteAdherenceOnOutOfAdherence()
		{
			var siteId = Guid.NewGuid();
			var persister = new FakeSiteOutOfAdherenceReadModelPersister();
			var target = new SiteOutOfAdherenceReadModelUpdater(persister);

			target.Handle(new PersonOutOfAdherenceEvent() { SiteId = siteId });

			persister.Get(siteId).Count.Should().Be(1);
		}

		[Test]
		public void ShouldExcludePersonGoingInAdherence()
		{
			var siteId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var persister = new FakeSiteOutOfAdherenceReadModelPersister();
			var target = new SiteOutOfAdherenceReadModelUpdater(persister);

			target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId, PersonId = personId });
			target.Handle(new PersonInAdherenceEvent { SiteId = siteId, PersonId = personId });

			persister.Get(siteId).Count.Should().Be(0);
		}

		[Test]
		public void ShouldIncludePersonGoingOutOfAdherence()
		{
			var siteId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var persister = new FakeSiteOutOfAdherenceReadModelPersister();
			var target = new SiteOutOfAdherenceReadModelUpdater(persister);

			target.Handle(new PersonInAdherenceEvent { SiteId = siteId, PersonId = personId });
			target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId, PersonId = personId });

			persister.Get(siteId).Count.Should().Be(1);
		}

		[Test]
		public void ShouldCountPersonAsOutOfAdherenceWhenGoingOutInOut()
		{
			var siteId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var persister = new FakeSiteOutOfAdherenceReadModelPersister();
			var target = new SiteOutOfAdherenceReadModelUpdater(persister);

			target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId, PersonId = personId });
			target.Handle(new PersonInAdherenceEvent { SiteId = siteId, PersonId = personId });
			target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId, PersonId = personId });

			persister.Get(siteId).Count.Should().Be(1);
		}

		[Test]
		public void ShouldCountPersonAsInAdherenceWhenGoingInOutIn()
		{
			var siteId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var persister = new FakeSiteOutOfAdherenceReadModelPersister();
			var target = new SiteOutOfAdherenceReadModelUpdater(persister);

			target.Handle(new PersonInAdherenceEvent { SiteId = siteId, PersonId = personId });
			target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId, PersonId = personId });
			target.Handle(new PersonInAdherenceEvent { SiteId = siteId, PersonId = personId });

			persister.Get(siteId).Count.Should().Be(0);
		}

		[Test]
		public void ShouldSummarizePersonsOutOfAdherence()
		{
			var persister = new FakeSiteOutOfAdherenceReadModelPersister();
			var target = new SiteOutOfAdherenceReadModelUpdater(persister);
			var siteId = Guid.NewGuid();
			var personId = Guid.NewGuid();

			target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId, PersonId = Guid.NewGuid() });
			target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId, PersonId = personId });
			target.Handle(new PersonInAdherenceEvent { SiteId = siteId, PersonId = personId });

			persister.Get(siteId).Count.Should().Be(1);
		}

		[Test]
		public void ShouldSummarizePersonsOutOfAdherenceForEachSite()
		{
			var siteId1 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();
			var persister = new FakeSiteOutOfAdherenceReadModelPersister();
			var target = new SiteOutOfAdherenceReadModelUpdater(persister);

			target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId1, PersonId = Guid.NewGuid() });
			target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId2, PersonId = Guid.NewGuid() });
			target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId1, PersonId = Guid.NewGuid() });

			persister.Get(siteId1).Count.Should().Be(2);
			persister.Get(siteId2).Count.Should().Be(1);
		}

		[Test]
		public void ShouldExcludePersonGoingInAdherenceForTheFirstTime()
		{
			var siteId = Guid.NewGuid();
			var persister = new FakeSiteOutOfAdherenceReadModelPersister();
			var target = new SiteOutOfAdherenceReadModelUpdater(persister);

			target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId, PersonId = Guid.NewGuid() });
			target.Handle(new PersonInAdherenceEvent { SiteId = siteId, PersonId = Guid.NewGuid() });

			persister.Get(siteId).Count.Should().Be(1);
		}

		[Test]
		public void ShouldNeverCountNegativePersonsBeingOutOfAdherence()
		{
			var persister = new FakeSiteOutOfAdherenceReadModelPersister();
			var target = new SiteOutOfAdherenceReadModelUpdater(persister);
			var siteId = Guid.NewGuid();

			target.Handle(new PersonInAdherenceEvent() { SiteId = siteId, PersonId = Guid.NewGuid() });
			target.Handle(new PersonInAdherenceEvent() { SiteId = siteId, PersonId = Guid.NewGuid() });

			persister.Get(siteId).Count.Should().Be(0);
		}

		[Test]
		public void ShouldUpdateAgentsOutOfAdherenceWhenFirstStateIsInAdherence()
		{
			var siteId = Guid.NewGuid();
			var persister = new FakeSiteOutOfAdherenceReadModelPersister();
			var target = new SiteOutOfAdherenceReadModelUpdater(persister);

			target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId, PersonId = Guid.NewGuid() });
			target.Handle(new PersonInAdherenceEvent { SiteId = siteId, PersonId = Guid.NewGuid() });

			persister.Get(siteId).Count.Should().Be(1);
		}

		[Test]
		public void ShouldPersistWithBusinessUnitIdOnOutOfAdherence()
		{
			var businessUnitId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var persister = new FakeSiteOutOfAdherenceReadModelPersister();
			var target = new SiteOutOfAdherenceReadModelUpdater(persister);

			target.Handle(new PersonOutOfAdherenceEvent() {SiteId = siteId, BusinessUnitId = businessUnitId});

			persister.GetForBusinessUnit(businessUnitId).Single().BusinessUnitId.Should().Be(businessUnitId);
		}

		[Test]
		public void ShouldPersistWithBusinessUnitIdOnInAdherence()
		{
			var businessUnitId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var persister = new FakeSiteOutOfAdherenceReadModelPersister();
			var target = new SiteOutOfAdherenceReadModelUpdater(persister);

			target.Handle(new PersonInAdherenceEvent() { SiteId = siteId, BusinessUnitId = businessUnitId });

			persister.GetForBusinessUnit(businessUnitId).Single().BusinessUnitId.Should().Be(businessUnitId);
		}

	}
	
}