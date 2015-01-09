using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	[TestFixture]
	public class SiteAdherenceReadModelUpdaterTest
	{
		[Test]
		public void PersonOutOfAdherenceEvent_ShouldIncreaseAgentsOutOfAdherenceForSite()
		{
			var siteId = Guid.NewGuid();
			var anotherId = Guid.NewGuid();
			var inAdherence = new PersonOutOfAdherenceEvent() { SiteId = siteId };
			var persister = new FakeSiteAdherencePersister();
			var target = new SiteAdherenceReadModelUpdater(persister);

			target.Handle(inAdherence);
			target.Handle(new PersonOutOfAdherenceEvent() { SiteId = anotherId });

			var result = persister.Get(siteId);

			result.AgentsOutOfAdherence.Should().Be(1);
		}

		[Test]
		public void PersonInAdherenceEvent_WhenThereAreAgentsOutOfAdherenceForSite_ShouldDecreaseAgentsOutOfAdherence()
		{
			var persister = new FakeSiteAdherencePersister();
			var target = new SiteAdherenceReadModelUpdater(persister);
			var siteId = Guid.NewGuid();

			target.Handle(new PersonOutOfAdherenceEvent() {SiteId = siteId });
			target.Handle(new PersonOutOfAdherenceEvent() {SiteId = siteId });
			target.Handle(new PersonInAdherenceEvent() {SiteId = siteId });

			persister.Get(siteId).AgentsOutOfAdherence.Should().Be(1);
		}

		[Test]
		public void PersonInAdherenceEvent_WhenNoOneInSiteIsOutOfAdherence_ShouldStillBeZero()
		{
			var persister = new FakeSiteAdherencePersister();
			var target = new SiteAdherenceReadModelUpdater(persister);
			var siteId = Guid.NewGuid();
			persister.Persist(new SiteAdherenceReadModel() { SiteId = siteId });
			
			target.Handle(new PersonInAdherenceEvent() { SiteId = siteId });

			persister.Get(siteId).AgentsOutOfAdherence.Should().Be(0);
		}
		[Test]
		public void PersonOutOfAdherenceEvent_ShouldSetTheBusinessUnitOnReadModel()
		{
			var businessUnitId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var outOfAdherenceEvent = new PersonOutOfAdherenceEvent() { SiteId = siteId, BusinessUnitId = businessUnitId };
			var persister = new FakeSiteAdherencePersister();
			var target = new SiteAdherenceReadModelUpdater(persister);

			target.Handle(outOfAdherenceEvent);
			
			persister.GetAll(businessUnitId).Count().Should().Be(1);

		}

		[Test]
		public void PersonInAdherenceEvent_ShouldSetTheBusinessUnitOnReadModel()
		{
			var businessUnitId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var inAdherence = new PersonInAdherenceEvent() { SiteId = siteId, BusinessUnitId = businessUnitId };
			var persister = new FakeSiteAdherencePersister();
			var target = new SiteAdherenceReadModelUpdater(persister);

			target.Handle(inAdherence);

			persister.GetAll(businessUnitId).Count().Should().Be(1);
		}
		
	}
}