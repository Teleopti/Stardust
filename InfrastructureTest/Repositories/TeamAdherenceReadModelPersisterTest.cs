using System;
using System.Linq;
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

		[Test]
		public void ModelShouldBeNullIfNotExists()
		{
			var model = Target.Get(Guid.NewGuid());

			model.Should().Be.Null();
		}

		[Test]
		public void ShouldGetTeamsForSite()
		{
			var siteId1 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var model1 = new TeamAdherenceReadModel {SiteId = siteId1, TeamId = teamId, AgentsOutOfAdherence = 1};
			var model2 = new TeamAdherenceReadModel {SiteId = siteId2, TeamId = teamId, AgentsOutOfAdherence = 1};
			Target.Persist(model1);
			Target.Persist(model2);
			var readModel = Target.GetForSite(siteId1);
			readModel.Single().AgentsOutOfAdherence.Should().Be(1);
			readModel.Single().TeamId.Should().Be(teamId);
		}
	}
}