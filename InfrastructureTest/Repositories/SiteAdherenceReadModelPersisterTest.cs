using System;
using System.Data.SqlClient;
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
		public void ShouldPersistIfPersonIdsAreMoreThan4000Characters()
		{
			const string longPersonIds = "There are 4001 characters having fake ids 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a6388";
			Target.Persist(new SiteOutOfAdherenceReadModel()
			{
				PersonIds = longPersonIds
			});
			Target.HasData().Should().Be.True();
		}

		[Test]
		public void ShouldUpdatePersonIdsToMakeItMoreThan4000Characters()
		{
			var siteId = Guid.NewGuid();
			var singlePersonId = Guid.NewGuid().ToString();
			const string longPersonIds = "There are 4001 characters having fake ids 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a638 5d487cf6-af83-4901-ba75-3a17f734a6388";
			
			Target.Persist(new SiteOutOfAdherenceReadModel()
			{
				PersonIds = singlePersonId,
				SiteId = siteId
			});
			Target.Persist(new SiteOutOfAdherenceReadModel()
			{
				PersonIds = longPersonIds,
				SiteId = siteId
			});

			var model = Target.Get(siteId);
			model.PersonIds.Should().Be(longPersonIds);
		}
	}
}