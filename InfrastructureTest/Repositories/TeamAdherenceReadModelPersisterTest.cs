using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, Category("LongRunning")]
	public class TeamAdherenceReadModelPersisterTest : IReadModelReadWriteTest
	{
		public ITeamAdherencePersister Target { get; set; }
		[Test]
		public void ShouldSaveReadModelForTeam()
		{
			var teamId = Guid.NewGuid();
			var model = new TeamAdherenceReadModel() {AgentsOutOfAdherence = 3,TeamId = teamId};

			Target.Persist(model);

			var savedModel = Target.Get(teamId);
			savedModel.AgentsOutOfAdherence.Should().Be.EqualTo(model.AgentsOutOfAdherence);
			savedModel.TeamId.Should().Be.EqualTo(model.TeamId);
		}

		[Test]
		public void ShouldUpdateReadModelIfExists()
		{
			var teamId = Guid.NewGuid();
			var model = new TeamAdherenceReadModel() { AgentsOutOfAdherence = 3, TeamId = teamId };

			Target.Persist(model);

			model.AgentsOutOfAdherence = 5;

			Target.Persist(model);

			var savedModel = Target.Get(teamId);
			savedModel.AgentsOutOfAdherence.Should().Be.EqualTo(5);
		}
	}
}