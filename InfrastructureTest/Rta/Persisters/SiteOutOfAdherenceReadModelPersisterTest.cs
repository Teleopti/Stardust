using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
{
	[TestFixture, Category("LongRunning")]
	[ReadModelUnitOfWorkTest]
	public class SiteOutOfAdherenceReadModelPersisterTest
	{
		public ISiteOutOfAdherenceReadModelPersister Target { get; set; }
		public ISiteOutOfAdherenceReadModelReader Reader { get; set; }

		[Test]
		public void ShouldPersist()
		{
			var siteId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var personId = Guid.NewGuid();

			Target.Persist(new SiteOutOfAdherenceReadModel()
			{
				Count = 21,
				SiteId = siteId,
				BusinessUnitId = businessUnitId,
				State = new[]
				{
					new SiteOutOfAdherenceReadModelState()
					{
						PersonId = personId
					}
				}
			});

			var model = Target.Get(siteId);
			model.Count.Should().Be(21);
			model.SiteId.Should().Be(siteId);
			model.BusinessUnitId.Should().Be(businessUnitId);
			model.State.Count().Should().Be(1);
			model.State.Single().PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldUpdateExistingModel()
		{
			var siteId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var personId = Guid.NewGuid();

			Target.Persist(new SiteOutOfAdherenceReadModel()
			{
				SiteId = siteId,
				State = new[] { new SiteOutOfAdherenceReadModelState() { PersonId = personId} }
			});
			Target.Persist(new SiteOutOfAdherenceReadModel()
			{
				Count = 5,
				SiteId = siteId,
				BusinessUnitId = businessUnitId,
				State = new[] { new SiteOutOfAdherenceReadModelState() { PersonId = personId} }
			});

			var model = Target.Get(siteId);
			model.Count.Should().Be(5);
			model.SiteId.Should().Be(siteId);
			model.BusinessUnitId.Should().Be(businessUnitId);
			model.State.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldGetNullIfNotExists()
		{
			var model = Target.Get(Guid.NewGuid());

			model.Should().Be.Null();
		}

		[Test]
		public void ShouldGetSiteOutOfAdherenceFromSiteIds()
		{
			var siteId1 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();
			var bu1A = Guid.NewGuid();

			Target.Persist(new SiteOutOfAdherenceReadModel { SiteId = siteId1, BusinessUnitId = bu1A, Count = 1 });
			Target.Persist(new SiteOutOfAdherenceReadModel { SiteId = siteId2, BusinessUnitId = bu1A, Count = 2 });
			
			var models = Reader.Read();
			models.Should().Have.Count.EqualTo(2);
			models.Single(x => x.SiteId == siteId1).Count.Should().Be(1);
			models.Single(x => x.SiteId == siteId2).Count.Should().Be(2);
		}

		[Test]
		public void ShouldKnowIfThereIsData()
		{
			Target.Persist(new SiteOutOfAdherenceReadModel ());

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
			Target.Persist(new SiteOutOfAdherenceReadModel());

			Target.Clear();

			Target.HasData().Should().Be.False();
		}

		[Test]
		public void ShouldPersistIfStatesAreMoreThan4000Characters()
		{
			var states = Enumerable.Range(0, 200).Select(i => new SiteOutOfAdherenceReadModelState()
			{
				OutOfAdherence = true,
				PersonId = Guid.NewGuid(),
				Time = DateTime.Now
			}).ToList();

			Target.Persist(new SiteOutOfAdherenceReadModel()
			{
				State = states
			});
			Target.HasData().Should().Be.True();
		}
	}
}