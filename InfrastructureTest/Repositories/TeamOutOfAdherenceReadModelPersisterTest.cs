using System;
using System.Linq;
using NHibernate.Exceptions;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, Category("LongRunning")]
	[ReadModelTest]
	public class TeamOutOfAdherenceReadModelPersisterTest
	{
		public ITeamOutOfAdherenceReadModelPersister Target { get; set; }

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

			Target.Persist(new TeamOutOfAdherenceReadModel
			{
				TeamId = teamId,
				State = new [] {new TeamOutOfAdherenceReadModelState(){AdherenceCounter = 1, PersonId = Guid.NewGuid()}}
			});
			Target.Persist(new TeamOutOfAdherenceReadModel
			{
				Count = 5, 
				TeamId = teamId,
				SiteId = siteId,
				State = new[] { new TeamOutOfAdherenceReadModelState() { AdherenceCounter = 2, PersonId = Guid.NewGuid() } }
			});

			var model = Target.Get(teamId);
			model.Count.Should().Be(5);
			model.SiteId.Should().Be(siteId);
			model.State.SingleOrDefault().AdherenceCounter.Should().Be(2);
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
			var teamId1 = Guid.NewGuid();
			var teamId2 = Guid.NewGuid();

			Target.Persist(new TeamOutOfAdherenceReadModel
			{
				SiteId = siteId1,
				TeamId = teamId1,
				Count = 1,
				State = new[] {new TeamOutOfAdherenceReadModelState() {AdherenceCounter = 2, PersonId = Guid.NewGuid()}}
			});
			Target.Persist(new TeamOutOfAdherenceReadModel
			{
				SiteId = siteId2,
				TeamId = teamId2,
				Count = 1,
				State = new[] {new TeamOutOfAdherenceReadModelState() {AdherenceCounter = 1, PersonId = Guid.NewGuid()}}
			});

			var readModel = Target.GetForSite(siteId1);
			readModel.Single().TeamId.Should().Be(teamId1);
			readModel.Single().State.SingleOrDefault().AdherenceCounter.Should().Be(2);
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
	}
}