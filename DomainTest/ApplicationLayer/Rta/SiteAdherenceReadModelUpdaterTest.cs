using System;
using System.Collections.Generic;
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
			var persister = new fakeSiteAdherencePersister();
			var target = new SiteAdherenceReadModelUpdater(persister);

			target.Handle(inAdherence);
			target.Handle(new PersonOutOfAdherenceEvent() { SiteId = anotherId });

			var result = persister.Get(siteId);

			result.AgentsOutOfAdherence.Should().Be(1);
		}

		[Test]
		public void PersonInAdherenceEvent_WhenThereAreAgentsOutOfAdherenceForSite_ShouldDecreaseAgentsOutOfAdherence()
		{
			var persister = new fakeSiteAdherencePersister();
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
			var persister = new fakeSiteAdherencePersister();
			var target = new SiteAdherenceReadModelUpdater(persister);
			var siteId = Guid.NewGuid();
			persister.Persist(new SiteAdherenceReadModel() { SiteId = siteId });
			
			target.Handle(new PersonInAdherenceEvent() { SiteId = siteId });

			persister.Get(siteId).AgentsOutOfAdherence.Should().Be(0);
		}

		private class fakeSiteAdherencePersister : ISiteAdherencePersister
		{
			private readonly List<SiteAdherenceReadModel> _models = new List<SiteAdherenceReadModel>();

			public void Persist(SiteAdherenceReadModel model)
			{
				var existing = _models.FirstOrDefault(m => m.SiteId == model.SiteId);
				if (existing != null)
				{
					existing.AgentsOutOfAdherence = model.AgentsOutOfAdherence;
				}
				else _models.Add(model);
			}

			public SiteAdherenceReadModel Get(Guid siteId)
			{
				return _models.FirstOrDefault(m => m.SiteId == siteId);
			}
		}
	}
}