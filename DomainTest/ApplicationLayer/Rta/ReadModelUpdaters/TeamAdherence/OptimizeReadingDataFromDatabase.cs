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
		public void ShouldExcludePersonChangingTeam()
		{
			var siteId = Guid.NewGuid();
			var originTeam = Guid.NewGuid();
			var destinationTeam = Guid.NewGuid();
			var movingPerson = Guid.NewGuid();
			Target.Handle(new PersonOutOfAdherenceEvent {TeamId = originTeam, PersonId = movingPerson, SiteId = siteId});

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = movingPerson,
				TeamId = destinationTeam,
				SiteId = siteId,
				PreviousTeam = originTeam,
				PreviousSite = siteId
			});

			Persister.Get(originTeam).Count.Should().Be(0);
		}
		
		[Test]
		public void ShouldIncludeInDestinationTeam()
		{
			var siteId = Guid.NewGuid();
			var destinationTeam = Guid.NewGuid();
			var movingPerson = Guid.NewGuid();
			Target.Handle(new PersonInAdherenceEvent { TeamId = destinationTeam, PersonId = Guid.NewGuid(), SiteId = siteId});

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = movingPerson,
				TeamId = destinationTeam,
				SiteId = siteId,
				PreviousTeam = Guid.NewGuid(),
				PreviousSite = siteId
			});
			Target.Handle(new PersonOutOfAdherenceEvent { TeamId = destinationTeam, PersonId = movingPerson });

			Persister.Get(destinationTeam).Count.Should().Be(1);
		}
	}
}