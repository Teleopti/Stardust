using System;
using System.Data.SqlClient;
using System.Linq;
using NHibernate.Exceptions;
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
			var personId = Guid.NewGuid();

			var  stateModel = new SiteOutOfAdherenceReadModelState()
			{
				Count = 9,
				PersonId = personId
			};
			Target.Persist( new SiteOutOfAdherenceReadModel()
			{
				Count = 21, 
				SiteId = siteId, 
				BusinessUnitId = businessUnitId,
				State = new[] { stateModel }
			});

			var model = Target.Get(siteId);
			model.Count.Should().Be(21);
			model.SiteId.Should().Be(siteId);
			model.BusinessUnitId.Should().Be(businessUnitId);
			model.State.Count().Should().Be(1);
			model.State.Single().PersonId.Should().Be(personId);
			model.State.Single().Count.Should().Be(9);
		}

		[Test]
		public void ShouldUpdateExistingModel()
		{
			var siteId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var personId = Guid.NewGuid();

			var stateModel = new SiteOutOfAdherenceReadModelState() {Count = 9,PersonId = personId};
			Target.Persist(new SiteOutOfAdherenceReadModel()
			{
				SiteId = siteId,
				State = new[] { stateModel }
			});
			stateModel.Count = 15;
			Target.Persist(new SiteOutOfAdherenceReadModel()
			{
				Count = 5,
				SiteId = siteId,
				BusinessUnitId = businessUnitId,
				State = new[] { stateModel }
			});

			var model = Target.Get(siteId);
			model.Count.Should().Be(5);
			model.SiteId.Should().Be(siteId);
			model.BusinessUnitId.Should().Be(businessUnitId);
			model.State.Single().Count.Should().Be(15);
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

	}
}