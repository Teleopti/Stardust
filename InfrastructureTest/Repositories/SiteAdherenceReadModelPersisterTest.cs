using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, Category("LongRunning")]
	[ReadModelTest]
	public class SiteAdherenceReadModelPersisterTest
	{
		public ISiteOutOfAdherenceReadModelPersister Target { get; set; }

		[Test]
		public void ShouldPersist()
		{
			var siteId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var personIds = Guid.NewGuid().ToString();

			Target.Persist( new SiteOutOfAdherenceReadModel()
			{
				Count = 21, 
				SiteId = siteId, 
				BusinessUnitId = businessUnitId,
				PersonIds = personIds
			});

			var model = Target.Get(siteId);
			model.Count.Should().Be(21);
			model.SiteId.Should().Be(siteId);
			model.BusinessUnitId.Should().Be(businessUnitId);
			model.PersonIds.Should().Be(personIds);
		}

		[Test]
		public void ShouldUpdateExistingModel()
		{
			var siteId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var personIds = Guid.NewGuid().ToString();

			Target.Persist(new SiteOutOfAdherenceReadModel()
			{
				SiteId = siteId
			});
			Target.Persist(new SiteOutOfAdherenceReadModel()
			{
				Count = 5,
				SiteId = siteId,
				BusinessUnitId = businessUnitId,
				PersonIds = personIds
			});

			var model = Target.Get(siteId);
			model.Count.Should().Be(5);
			model.SiteId.Should().Be(siteId);
			model.BusinessUnitId.Should().Be(businessUnitId);
			model.PersonIds.Should().Be(personIds);
		}

		[Test]
		public void ShouldGetNullIfNotExists()
		{
			var model = Target.Get(Guid.NewGuid());

			model.Should().Be.Null();
		}

		[Test]
		public void ShouldGetSitesForBusinessUnit()
		{
			var site1 = Guid.NewGuid();
			var site2 = Guid.NewGuid();
			var site3 = Guid.NewGuid();
			var buId1 = Guid.NewGuid();

			Target.Persist(new SiteOutOfAdherenceReadModel() { SiteId = site1, BusinessUnitId = buId1});
			Target.Persist(new SiteOutOfAdherenceReadModel() { SiteId = site2, BusinessUnitId = buId1 });
			Target.Persist(new SiteOutOfAdherenceReadModel() { SiteId = site3, BusinessUnitId = Guid.NewGuid() });

			var models = Target.GetForBusinessUnit(buId1);
			models.Select(x => x.SiteId).Should().Have.SameValuesAs(new[] {site1, site2});
		}

		[Test]
		public void ShouldKnowIfThereIsData()
		{
			Target.Persist(new SiteOutOfAdherenceReadModel { SiteId = Guid.NewGuid()});

			Target.HasData().Should().Be.True();
		}

		[Test, Ignore]
		public void ShouldKnowIfThereIsNoData()
		{
			Target.HasData().Should().Be.False();
		}
	}
}