using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.TeamAdherence
{
	[ReadModelUpdaterTest]
	[TestFixture]
	public class OptimizeReadingDataFromDatabase
	{
		public FakeTeamOutOfAdherenceReadModelPersister Persister;
		public TeamOutOfAdherenceReadModelUpdater Target;

		[Test]
		public void ShouldExcludePersonFromPreviousTeams()
		{
			var businessUnitId = Guid.NewGuid();
			var previousSiteId = Guid.NewGuid();
			var previousTeam1 = Guid.NewGuid();
			var previousTeam2 = Guid.NewGuid();
			var destinationSite = Guid.NewGuid();
			var movingPerson = Guid.NewGuid();
			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = movingPerson,
				BusinessUnitId = businessUnitId,
				SiteId = previousSiteId,
				TeamId = previousTeam1,
			});
			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = movingPerson,
				BusinessUnitId = businessUnitId,
				SiteId = previousSiteId,
				TeamId = previousTeam2,
			});

			Target.Handle(new PersonAssociationChangedEvent
			{
				Version = 2,
				PersonId = movingPerson,
				BusinessUnitId = businessUnitId,
				SiteId = destinationSite,
				PreviousAssociation = new[]
				{
					new Association
					{
						BusinessUnitId = businessUnitId,
						SiteId = previousSiteId,
						TeamId = previousTeam1
					},
					new Association
					{
						BusinessUnitId = businessUnitId,
						SiteId = previousSiteId,
						TeamId = previousTeam2
					}
				}
			});

			Persister.Get(previousTeam1).Count.Should().Be(0);
			Persister.Get(previousTeam2).Count.Should().Be(0);
		}

		[Test]
		public void ShouldIncludeInNewTeam()
		{
			var siteId = Guid.NewGuid();
			var originTeam = Guid.NewGuid();
			var destinationTeam = Guid.NewGuid();
			var movingPerson = Guid.NewGuid();
			Target.Handle(new PersonOutOfAdherenceEvent { PersonId = movingPerson, SiteId = siteId, TeamId = originTeam, });

			Target.Handle(new PersonAssociationChangedEvent
			{
				Version = 2,
				PersonId = movingPerson,
				SiteId = siteId,
				TeamId = destinationTeam
			});
			Target.Handle(new PersonOutOfAdherenceEvent { PersonId = movingPerson, SiteId = siteId, TeamId = destinationTeam, });

			Persister.Get(destinationTeam).Count.Should().Be(1);
		}


		[Test]
		public void ShouldExcludeWhenTerminated()
		{
			var previousSite = Guid.NewGuid();
			var previousTeam = Guid.NewGuid();
			var movingPerson = Guid.NewGuid();
			Target.Handle(new PersonOutOfAdherenceEvent
			{
				PersonId = movingPerson,
				SiteId = previousSite,
				TeamId = previousTeam,
			});

			Target.Handle(new PersonAssociationChangedEvent
			{
				Version = 2,
				PersonId = movingPerson,
				PreviousAssociation = new[]
				{
					new Association
					{
						SiteId = previousSite,
						TeamId = previousTeam
					}
				}
			});

			Persister.Get(previousTeam).Count.Should().Be(0);
		}
	}
}