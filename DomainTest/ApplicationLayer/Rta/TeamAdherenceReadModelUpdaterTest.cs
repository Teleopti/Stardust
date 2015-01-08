using System;
using System.Security.Policy;
using NUnit.Framework;
using Rhino.Mocks.Constraints;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
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

			persister.Get(teamId).AgentsOutOfAdherence.Should().Be(0);
		}

		[Test]
		public void ShouldUpdateTeamAdherenceForOutOfAdherence()
		{
			var teamId = Guid.NewGuid();
			var persister = new FakeTeamAdherencePersister();
			var target = new TeamAdherenceReadModelUpdater(persister);

			target.Handle(new PersonOutOfAdherenceEvent() { TeamId = teamId });

			persister.Get(teamId).AgentsOutOfAdherence.Should().Be(1);
		}


		[Test]
		public void ShouldSummarizeAdherenceForTeam()
		{
			var teamId1 = Guid.NewGuid();
			var teamId2 = Guid.NewGuid();

			var persister = new FakeTeamAdherencePersister();
			var target = new TeamAdherenceReadModelUpdater(persister);

			target.Handle(new PersonOutOfAdherenceEvent() { TeamId = teamId1 });
			target.Handle(new PersonOutOfAdherenceEvent() { TeamId = teamId1 });
			target.Handle(new PersonOutOfAdherenceEvent() { TeamId = teamId2 });
			target.Handle(new PersonOutOfAdherenceEvent() { TeamId = teamId1 });

			persister.Get(teamId1).AgentsOutOfAdherence.Should().Be(3);
		}


		[Test]
		public void ShouldSummarizeAdherenceForTeamInAndOut()
		{
			var teamId1 = Guid.NewGuid();
			var persister = new FakeTeamAdherencePersister();
			var target = new TeamAdherenceReadModelUpdater(persister);

			target.Handle(new PersonOutOfAdherenceEvent {TeamId = teamId1});
			target.Handle(new PersonInAdherenceEvent {TeamId = teamId1});

			persister.Get(teamId1).AgentsOutOfAdherence.Should().Be(0);
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
			var persister = new fakeTeamAdherencePersister();
			var target = new TeamAdherenceReadModelUpdater(persister);

			target.Handle(new PersonInAdherenceEvent() { TeamId = teamId, SiteId = siteId});

			persister.GetForSite(siteId).Single().TeamId.Should().Be(teamId);
		}

		[Test]
		public void ShouldUpdateTeamOutOfAdherenceWithSiteId()
		{
			var teamId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var persister = new fakeTeamAdherencePersister();
			var target = new TeamAdherenceReadModelUpdater(persister);

			target.Handle(new PersonOutOfAdherenceEvent{ TeamId = teamId, SiteId = siteId});

			persister.GetForSite(siteId).Single().TeamId.Should().Be(teamId);
		}


		
			}

			public IEnumerable<TeamAdherenceReadModel> GetForSite(Guid siteId)
			{
				return _models.Where(x => x.SiteId == siteId);
	}
}