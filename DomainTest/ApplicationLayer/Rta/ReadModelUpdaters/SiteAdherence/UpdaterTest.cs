using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.SiteAdherence
{
	[AdherenceTest]
	[TestFixture]
	public class UpdaterTest
	{
		public FakeSiteOutOfAdherenceReadModelPersister Persister;
		public SiteOutOfAdherenceReadModelUpdater Target;

		[Test]
		public void ShouldPersistSiteAdherenceOnInAdherence()
		{
			var siteId = Guid.NewGuid();

			Target.Handle(new PersonInAdherenceEvent { SiteId = siteId });

			Persister.Get(siteId).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPersistSiteAdherenceOnOutOfAdherence()
		{
			var siteId = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent() { SiteId = siteId });

			Persister.Get(siteId).Count.Should().Be(1);
		}

		[Test]
		public void ShouldExcludePersonGoingInAdherence()
		{
			var siteId = Guid.NewGuid();
			var personId = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId, PersonId = personId });
			Target.Handle(new PersonInAdherenceEvent { SiteId = siteId, PersonId = personId });

			Persister.Get(siteId).Count.Should().Be(0);
		}

		[Test]
		public void ShouldIncludePersonGoingOutOfAdherence()
		{
			var siteId = Guid.NewGuid();
			var personId = Guid.NewGuid();

			Target.Handle(new PersonInAdherenceEvent { SiteId = siteId, PersonId = personId });
			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId, PersonId = personId });

			Persister.Get(siteId).Count.Should().Be(1);
		}

		[Test]
		public void ShouldCountPersonAsOutOfAdherenceWhenGoingOutInOut()
		{
			var siteId = Guid.NewGuid();
			var personId = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId, PersonId = personId });
			Target.Handle(new PersonInAdherenceEvent { SiteId = siteId, PersonId = personId });
			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId, PersonId = personId });

			Persister.Get(siteId).Count.Should().Be(1);
		}

		[Test]
		public void ShouldCountPersonAsInAdherenceWhenGoingInOutIn()
		{
			var siteId = Guid.NewGuid();
			var personId = Guid.NewGuid();

			Target.Handle(new PersonInAdherenceEvent { SiteId = siteId, PersonId = personId });
			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId, PersonId = personId });
			Target.Handle(new PersonInAdherenceEvent { SiteId = siteId, PersonId = personId });

			Persister.Get(siteId).Count.Should().Be(0);
		}
		
		[Test]
		public void ShouldCountPersonAsNeutralAdherenceWhenGoingOutNeutral()
		{
			var siteId = Guid.NewGuid();
			var personId = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId, PersonId = personId });
			Target.Handle(new PersonNeutralAdherenceEvent { SiteId = siteId, PersonId = personId });

			Persister.Get(siteId).Count.Should().Be(0);
		}

		[Test]
		public void ShouldCountPersonAsOutOfAdherenceWhenGoingOutNeutralOut()
		{
			var siteId = Guid.NewGuid();
			var personId = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId, PersonId = personId });
			Target.Handle(new PersonNeutralAdherenceEvent { SiteId = siteId, PersonId = personId });
			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId, PersonId = personId });

			Persister.Get(siteId).Count.Should().Be(1);
		}

		[Test]
		public void ShouldSummarizePersonsOutOfAdherence()
		{
			var siteId = Guid.NewGuid();
			var personId = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId, PersonId = Guid.NewGuid() });
			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId, PersonId = personId });
			Target.Handle(new PersonInAdherenceEvent { SiteId = siteId, PersonId = personId });

			Persister.Get(siteId).Count.Should().Be(1);
		}
		
		[Test]
		public void ShouldSummarizePersonsOutOfAdherenceForEachSite()
		{
			var siteId1 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId1, PersonId = Guid.NewGuid() });
			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId2, PersonId = Guid.NewGuid() });
			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId1, PersonId = Guid.NewGuid() });

			Persister.Get(siteId1).Count.Should().Be(2);
			Persister.Get(siteId2).Count.Should().Be(1);
		}

		[Test]
		public void ShouldExcludePersonGoingInAdherenceForTheFirstTime()
		{
			var siteId = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId, PersonId = Guid.NewGuid() });
			Target.Handle(new PersonInAdherenceEvent { SiteId = siteId, PersonId = Guid.NewGuid() });

			Persister.Get(siteId).Count.Should().Be(1);
		}

		[Test]
		public void ShouldNeverCountNegativePersonsBeingOutOfAdherence()
		{
			var siteId = Guid.NewGuid();

			Target.Handle(new PersonInAdherenceEvent { SiteId = siteId, PersonId = Guid.NewGuid() });
			Target.Handle(new PersonInAdherenceEvent { SiteId = siteId, PersonId = Guid.NewGuid() });

			Persister.Get(siteId).Count.Should().Be(0);
		}

		[Test]
		public void ShouldUpdateAgentsOutOfAdherenceWhenFirstStateIsInAdherence()
		{
			var siteId = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId, PersonId = Guid.NewGuid() });
			Target.Handle(new PersonInAdherenceEvent { SiteId = siteId, PersonId = Guid.NewGuid() });

			Persister.Get(siteId).Count.Should().Be(1);
		}

		[Test]
		public void ShouldPersistWithBusinessUnitIdOnOutOfAdherence()
		{
			var businessUnitId = Guid.NewGuid();
			var siteId = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent {SiteId = siteId, BusinessUnitId = businessUnitId});

			Persister.GetForBusinessUnit(businessUnitId).Single().BusinessUnitId.Should().Be(businessUnitId);
		}

		[Test]
		public void ShouldPersistWithBusinessUnitIdOnInAdherence()
		{
			var businessUnitId = Guid.NewGuid();
			var siteId = Guid.NewGuid();

			Target.Handle(new PersonInAdherenceEvent { SiteId = siteId, BusinessUnitId = businessUnitId });

			Persister.GetForBusinessUnit(businessUnitId).Single().BusinessUnitId.Should().Be(businessUnitId);
		}
	}
	
}