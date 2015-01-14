using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	[TestFixture]
	public class TeamAdherenceReadModelUpdaterTest
	{
		[Test]
		public void ShouldUpdateTeamAdherence()
		{
			var teamId = Guid.NewGuid();
			var persister = new FakeTeamOutOfAdherenceReadModelPersister();
			var target = new TeamOutOfAdherenceReadModelUpdater(persister);

			target.Handle(new PersonInAdherenceEvent() {TeamId = teamId});

			persister.Get(teamId).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldUpdateAgentsOutOfAdherence()
		{
			var teamId = Guid.NewGuid();
			var persister = new FakeTeamOutOfAdherenceReadModelPersister();
			var target = new TeamOutOfAdherenceReadModelUpdater(persister);

			target.Handle(new PersonOutOfAdherenceEvent() { TeamId = teamId });

			persister.Get(teamId).Count.Should().Be(1);
		}

		[Test]
		public void ShouldSummarizeOutOfAdherenceForEachTeam()
		{
			var teamId1 = Guid.NewGuid();
			var teamId2 = Guid.NewGuid();

			var persister = new FakeTeamOutOfAdherenceReadModelPersister();
			var target = new TeamOutOfAdherenceReadModelUpdater(persister);

			target.Handle(new PersonOutOfAdherenceEvent() { TeamId = teamId1 });
			target.Handle(new PersonOutOfAdherenceEvent() { TeamId = teamId2 });
			target.Handle(new PersonOutOfAdherenceEvent() { TeamId = teamId1 });

			persister.Get(teamId1).Count.Should().Be(2);
			persister.Get(teamId2).Count.Should().Be(1);
		}

		[Test]
		public void ShouldSummarizeOutOfAdherenceForTeamInAndOut()
		{
			var teamId = Guid.NewGuid();
			var persister = new FakeTeamOutOfAdherenceReadModelPersister();
			var target = new TeamOutOfAdherenceReadModelUpdater(persister);

			target.Handle(new PersonOutOfAdherenceEvent {TeamId = teamId});
			target.Handle(new PersonOutOfAdherenceEvent {TeamId = teamId});
			target.Handle(new PersonInAdherenceEvent {TeamId = teamId});

			persister.Get(teamId).Count.Should().Be(1);
		}

		[Test]
		public void ShouldUpdateAgentsOutOfAdherenceWhenFirstStateIsInAdherence()
		{
			var teamId = Guid.NewGuid();
			var persister = new FakeTeamOutOfAdherenceReadModelPersister();
			var target = new TeamOutOfAdherenceReadModelUpdater(persister);

			target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId, PersonId = Guid.NewGuid()});
			target.Handle(new PersonInAdherenceEvent { TeamId = teamId, PersonId = Guid.NewGuid()});

			persister.Get(teamId).Count.Should().Be(1);
		}

		[Test]
		public void ShouldNeverSetNegativeAdherence()
		{
			var teamId1 = Guid.NewGuid();
			var persister = new FakeTeamOutOfAdherenceReadModelPersister();
			var target = new TeamOutOfAdherenceReadModelUpdater(persister);

			target.Handle(new PersonInAdherenceEvent { TeamId = teamId1 });
			target.Handle(new PersonInAdherenceEvent { TeamId = teamId1 });

			persister.Get(teamId1).Count.Should().Be(0);
		}

		[Test]
		public void ShouldUpdateTeamInAdherenceWithSiteId()
		{
			var teamId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var persister = new FakeTeamOutOfAdherenceReadModelPersister();
			var target = new TeamOutOfAdherenceReadModelUpdater(persister);

			target.Handle(new PersonInAdherenceEvent() { TeamId = teamId, SiteId = siteId});

			persister.GetForSite(siteId).Single().TeamId.Should().Be(teamId);
		}

		[Test]
		public void ShouldUpdateTeamOutOfAdherenceWithSiteId()
		{
			var teamId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var persister = new FakeTeamOutOfAdherenceReadModelPersister();
			var target = new TeamOutOfAdherenceReadModelUpdater(persister);

			target.Handle(new PersonOutOfAdherenceEvent{ TeamId = teamId, SiteId = siteId});

			persister.GetForSite(siteId).Single().TeamId.Should().Be(teamId);
		}


	}
}