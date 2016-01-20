using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.SiteAdherence
{
	[ReadModelUpdaterTest]
	[TestFixture]
	[Toggle(Toggles.RTA_TerminatedPersons_36042)]
	public class PersonTerminatedTest
	{
		public FakeSiteOutOfAdherenceReadModelPersister Persister;
		public SiteOutOfAdherenceReadModelUpdater Target;

		[Test]
		public void ShouldExludeTerminatedPerson()
		{
			var siteId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId, PersonId = personId });

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = personId,
				TeamId = null
			});

			Persister.Get(siteId).Count.Should().Be(0);
		}

		[Test]
		public void ShouldExludeFromAnySite()
		{
			var siteId1 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId1, PersonId = personId });
			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId2, PersonId = personId });

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = personId,
				TeamId = null
			});

			Persister.Get(siteId1).Count.Should().Be(0);
			Persister.Get(siteId2).Count.Should().Be(0);
		}

	}
}