using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.SiteAdherence
{
	[ReadModelUpdaterTest]
	[TestFixture]
	public class OptimizeReadingDataFromDatabase
	{
		public FakeSiteOutOfAdherenceReadModelPersister Persister;
		public SiteOutOfAdherenceReadModelUpdater Target;

		[Test]
		public void ShouldExcludePersonChangingSite_SiteChanged()
		{
			var businessUnitId = Guid.NewGuid();
			var originSite = Guid.NewGuid();
			var destinationSite = Guid.NewGuid();
			var movingPerson = Guid.NewGuid();

			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = originSite, PersonId = movingPerson, BusinessUnitId = businessUnitId });
			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = movingPerson,
				SiteId = destinationSite,
				PreviousSite = originSite,
				BusinessUnitId = businessUnitId
			});

			Persister.Get(originSite).Count.Should().Be(0);
		}
		
		[Test]
		public void ShouldIncludeInDestinationSite_SiteChanged()
		{
			var businessUnitId = Guid.NewGuid();
			var originSite = Guid.NewGuid();
			var destinationSite = Guid.NewGuid();
			var movingPerson = Guid.NewGuid();
			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = originSite, PersonId = movingPerson, BusinessUnitId = businessUnitId });

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = movingPerson,
				BusinessUnitId = businessUnitId,
				SiteId = destinationSite,
				PreviousSite = originSite
			});
			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = destinationSite, PersonId = movingPerson, BusinessUnitId = businessUnitId });

			Persister.Get(destinationSite).Count.Should().Be(1);
			Persister.Get(originSite).Count.Should().Be(0);
		}


		[Test]
		public void ShouldExludeFromAnyTeam_Terminated()
		{
			var siteId1 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId1, PersonId = personId });
			Target.Handle(new PersonOutOfAdherenceEvent { SiteId = siteId2, PersonId = personId });

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = personId,
				TeamId = null,
				PreviousSites = new[] { siteId1, siteId2 }
			});

			Persister.Get(siteId1).Count.Should().Be(0);
			Persister.Get(siteId2).Count.Should().Be(0);
		}

		[Test]
		public void ShouldWorkWhenPreviousTeamDoesNotHaveAModel_Terminated()
		{
			Target.Handle(new PersonAssociationChangedEvent { PersonId = Guid.NewGuid(), PreviousSites = new[] { Guid.NewGuid() } });
		}
	}
}