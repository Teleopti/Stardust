using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	[TestFixture]
	public class TeamAdherenceReadModelUpdaterTest
	{
		[Test]
		public void ShouldUpdateTeamAdherence()
		{
			var teamId = Guid.NewGuid();
			var persister = new FakeTeamAdherencePersister();
			var target = new TeamAdherenceReadModelUpdater(persister);

			target.Handle(new PersonInAdherenceEvent() {TeamId = teamId});

			persister.Get(teamId).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldUpdateAgentsOutOfAdherence()
		{
			var teamId = Guid.NewGuid();
			var persister = new FakeTeamAdherencePersister();
			var target = new TeamAdherenceReadModelUpdater(persister);

			target.Handle(new PersonOutOfAdherenceEvent() { TeamId = teamId });

			persister.Get(teamId).AgentsOutOfAdherence.Should().Be(1);
		}

		[Test]
		public void ShouldSummarizeOutOfAdherenceForEachTeam()
		{
			var teamId1 = Guid.NewGuid();
			var teamId2 = Guid.NewGuid();

			var persister = new FakeTeamAdherencePersister();
			var target = new TeamAdherenceReadModelUpdater(persister);

			target.Handle(new PersonOutOfAdherenceEvent() { TeamId = teamId1 });
			target.Handle(new PersonOutOfAdherenceEvent() { TeamId = teamId2 });
			target.Handle(new PersonOutOfAdherenceEvent() { TeamId = teamId1 });

			persister.Get(teamId1).AgentsOutOfAdherence.Should().Be(2);
			persister.Get(teamId2).AgentsOutOfAdherence.Should().Be(1);
		}

		[Test]
		public void ShouldSummarizeOutOfAdherenceForTeamInAndOut()
		{
			var teamId = Guid.NewGuid();
			var persister = new FakeTeamAdherencePersister();
			var target = new TeamAdherenceReadModelUpdater(persister);

			target.Handle(new PersonOutOfAdherenceEvent {TeamId = teamId});
			target.Handle(new PersonOutOfAdherenceEvent {TeamId = teamId});
			target.Handle(new PersonInAdherenceEvent {TeamId = teamId});

			persister.Get(teamId).AgentsOutOfAdherence.Should().Be(1);
		}

		[Test, Ignore]
		public void ShouldUpdateAgentsOutOfAdherenceWhenFirstStateIsInAdherence()
		{
			var teamId = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var persister = new FakeTeamAdherencePersister();
			var target = new TeamAdherenceReadModelUpdater(persister);

			target.Handle(new PersonOutOfAdherenceEvent { TeamId = teamId, PersonId = personId1});
			target.Handle(new PersonInAdherenceEvent { TeamId = teamId, PersonId = personId2});

			persister.Get(teamId).AgentsOutOfAdherence.Should().Be(1);
		}

		[Test]
		public void ShouldNeverSetNegativeAdherence()
		{
			var teamId1 = Guid.NewGuid();
			var persister = new FakeTeamAdherencePersister();
			var target = new TeamAdherenceReadModelUpdater(persister);

			target.Handle(new PersonInAdherenceEvent { TeamId = teamId1 });
			target.Handle(new PersonInAdherenceEvent { TeamId = teamId1 });

			persister.Get(teamId1).AgentsOutOfAdherence.Should().Be(0);
		}

		[Test]
		public void ShouldUpdateTeamInAdherenceWithSiteId()
		{
			var teamId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var persister = new FakeTeamAdherencePersister();
			var target = new TeamAdherenceReadModelUpdater(persister);

			target.Handle(new PersonInAdherenceEvent() { TeamId = teamId, SiteId = siteId});

			persister.GetForSite(siteId).Single().TeamId.Should().Be(teamId);
		}

		[Test]
		public void ShouldUpdateTeamOutOfAdherenceWithSiteId()
		{
			var teamId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var persister = new FakeTeamAdherencePersister();
			var target = new TeamAdherenceReadModelUpdater(persister);

			target.Handle(new PersonOutOfAdherenceEvent{ TeamId = teamId, SiteId = siteId});

			persister.GetForSite(siteId).Single().TeamId.Should().Be(teamId);
		}


	}
}