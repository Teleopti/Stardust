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
	public class TeamAdherenceReadModelUpdaterTest
	{
		[Test]
		public void ShouldUpdateTeamAdherence()
		{
			var teamId = Guid.NewGuid();
			var persister = new FakeTeamAdherencepersister();
			var target = new TeamAdherenceReadModelUpdater(persister);

			target.Handle(new PersonInAdherenceEvent() {TeamId = teamId});

			persister.Get(teamId).AgentsOutOfAdherence.Should().Be(0);
		}

		[Test]
		public void ShouldUpdateTeamAdherenceForOutOfAdherence()
		{
			var teamId = Guid.NewGuid();
			var persister = new FakeTeamAdherencepersister();
			var target = new TeamAdherenceReadModelUpdater(persister);

			target.Handle(new PersonOutOfAdherenceEvent() { TeamId = teamId });

			persister.Get(teamId).AgentsOutOfAdherence.Should().Be(1);
		}


		[Test]
		public void ShouldSummarizeAdherenceForTeam()
		{
			var teamId1 = Guid.NewGuid();
			var teamId2 = Guid.NewGuid();

			var persister = new FakeTeamAdherencepersister();
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
			var persister = new FakeTeamAdherencepersister();
			var target = new TeamAdherenceReadModelUpdater(persister);

			target.Handle(new PersonOutOfAdherenceEvent {TeamId = teamId1});
			target.Handle(new PersonInAdherenceEvent {TeamId = teamId1});

			persister.Get(teamId1).AgentsOutOfAdherence.Should().Be(0);
		}

		[Test]
		public void ShouldNeverSetNegativeAdherence()
		{
			var teamId1 = Guid.NewGuid();
			var persister = new FakeTeamAdherencepersister();
			var target = new TeamAdherenceReadModelUpdater(persister);

			target.Handle(new PersonInAdherenceEvent { TeamId = teamId1 });
			target.Handle(new PersonInAdherenceEvent { TeamId = teamId1 });

			persister.Get(teamId1).AgentsOutOfAdherence.Should().Be(0);
		}
	}

	public class FakeTeamAdherencepersister : ITeamAdherencepersister
	{

		private readonly List<TeamAdherenceReadModel> _models = new List<TeamAdherenceReadModel>();  
		
		public void Persist(TeamAdherenceReadModel model)
		{
			var existing = _models.FirstOrDefault(m => m.TeamId == model.TeamId);
			if (existing != null)
			{
				existing.AgentsOutOfAdherence = model.AgentsOutOfAdherence;
			}
			else _models.Add(model);
		}

		public TeamAdherenceReadModel Get(Guid teamId)
		{
			return _models.FirstOrDefault(m => m.TeamId == teamId);
		}
	}
}