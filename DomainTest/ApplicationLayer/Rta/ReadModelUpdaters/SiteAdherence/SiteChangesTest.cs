using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.SiteAdherence
{
	[ReadModelUpdaterTest]
	[TestFixture]
	[Toggle(Toggles.RTA_TeamChanges_36043)]
	public class SiteChangesTest
	{
		public FakeSiteOutOfAdherenceReadModelPersister Persister;
		public SiteOutOfAdherenceReadModelUpdater Target;

		[Test]
		public void ShouldExcludePersonChangingSite()
		{
			var originSite = Guid.NewGuid();
			var destinationSite = Guid.NewGuid();
			var movingPerson = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = originSite, PersonId = movingPerson });
			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = movingPerson,
				SiteId = destinationSite
			});

			Persister.Get(originSite).Count.Should().Be(0);
		}

		[Test]
		public void ShouldIncludeInDestinationSite()
		{
			var destinationSite = Guid.NewGuid();
			var movingPerson = Guid.NewGuid();
			Target.Handle(new PersonInAdherenceEvent { SiteId = destinationSite, PersonId = Guid.NewGuid() });

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = movingPerson,
				SiteId = destinationSite
			});
			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = destinationSite, PersonId = movingPerson });

			Persister.Get(destinationSite).Count.Should().Be(1);
		}
		
	}
}