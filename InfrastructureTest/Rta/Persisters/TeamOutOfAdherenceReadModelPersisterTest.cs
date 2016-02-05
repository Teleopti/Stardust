using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
{
	[TestFixture]
	[ReadModelUnitOfWorkTest]
	public class TeamOutOfAdherenceReadModelPersisterTest
	{
		public ITeamOutOfAdherenceReadModelPersister Target { get; set; }
		public ITeamOutOfAdherenceReadModelReader Reader { get; set; }

		[Test]
		public void ShouldPersist()
		{
			var teamId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			Target.Persist(new TeamOutOfAdherenceReadModel()
			{
				TeamId = teamId,
				SiteId = siteId,
				Count = 3
			});

			var model = Target.Get(teamId);
			model.Count.Should().Be(3);
			model.TeamId.Should().Be(teamId);
			model.SiteId.Should().Be(siteId);
		}

		[Test]
		public void ShouldUpdateExistingModel()
		{
			var teamId = Guid.NewGuid();
			var siteId = Guid.NewGuid();

			var model = new TeamOutOfAdherenceReadModel
			{
				TeamId = teamId,
				State = new [] {new TeamOutOfAdherenceReadModelState(){ PersonId = Guid.NewGuid()}}
			};
			Target.Persist(model);

			model.Count = 5;
			model.TeamId = teamId;
			model.SiteId = siteId;
			model.State = new[] {new TeamOutOfAdherenceReadModelState() {PersonId = Guid.NewGuid()}};
			Target.Persist(model);

			var actual = Target.Get(teamId);
			actual.Count.Should().Be(5);
			actual.TeamId.Should().Be(teamId);
			actual.SiteId.Should().Be(siteId);
			actual.State.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldGetNullIfNotExists()
		{
			var model = Target.Get(Guid.NewGuid());

			model.Should().Be.Null();
		}

		[Test]
		public void ShouldGetTeamsForSite()
		{
			var siteId1 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();
			var teamId1A = Guid.NewGuid();
			var teamId1B = Guid.NewGuid();
			var teamId2A = Guid.NewGuid();

			Target.Persist(new TeamOutOfAdherenceReadModel
			{
				SiteId = siteId1,
				TeamId = teamId1A,
				Count = 1
			});
			Target.Persist(new TeamOutOfAdherenceReadModel
			{
				SiteId = siteId1,
				TeamId = teamId1B,
				Count = 2
			});
			Target.Persist(new TeamOutOfAdherenceReadModel
			{
				SiteId = siteId2,
				TeamId = teamId2A,
				Count = 3
			});

			var models = Reader.Read(siteId1);
			models.Single(x => x.TeamId == teamId1A).Count.Should().Be(1);
			models.Single(x => x.TeamId == teamId1B).Count.Should().Be(2);
		}

		[Test]
		public void ShouldKnowIfThereIsData()
		{
			Target.Persist(new TeamOutOfAdherenceReadModel());

			Target.HasData().Should().Be.True();
		}

		[Test]
		public void ShouldKnowIfThereIsNoData()
		{
			Target.HasData().Should().Be.False();
		}

		[Test]
		public void ShouldClear()
		{
			Target.Persist(new TeamOutOfAdherenceReadModel(){State = new TeamOutOfAdherenceReadModelState[]{}});

			Target.Clear();

			Target.HasData().Should().Be.False();
		}

		[Test]
		public void ShouldPersistIfStatesAreMoreThan4000Characters()
		{
			var states = Enumerable.Range(0, 200).Select(i => new TeamOutOfAdherenceReadModelState()
			{
				OutOfAdherence = true,
				PersonId = Guid.NewGuid(),
				Time = DateTime.Now
			}).ToList();

			Target.Persist(new TeamOutOfAdherenceReadModel()
			{
				State = states
			});
			Target.HasData().Should().Be.True();
		}

		[Test]
		public void ShouldGetAll()
		{
			var team1 = Guid.NewGuid();
			var team2 = Guid.NewGuid();
			Target.Persist(new TeamOutOfAdherenceReadModel {TeamId = team1});
			Target.Persist(new TeamOutOfAdherenceReadModel {TeamId = team2});

			var models = Target.GetAll();

			models.Single(x => x.TeamId == team1).TeamId.Should().Be(team1);
			models.Single(x => x.TeamId == team2).TeamId.Should().Be(team2);
		}
	}

}