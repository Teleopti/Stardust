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
		public ISiteAdherencePersister Target { get; set; }
		[Test]
		public void ShouldSaveReadModelForSite()
		{
			var siteId = Guid.NewGuid();
			var model = new SiteAdherenceReadModel() { AgentsOutOfAdherence = 21, SiteId = siteId};

			Target.Persist(model);

			var savedModel = Target.Get(siteId);
			savedModel.AgentsOutOfAdherence.Should().Be.EqualTo(model.AgentsOutOfAdherence);
			savedModel.SiteId.Should().Be.EqualTo(model.SiteId);
		}

		[Test]
		public void ShouldUpdateReadModelIfExists()
		{
			var siteId = Guid.NewGuid();
			var model = new SiteAdherenceReadModel() { AgentsOutOfAdherence = 3, SiteId = siteId };

			Target.Persist(model);

			model.AgentsOutOfAdherence = 5;

			Target.Persist(model);

			var savedModel = Target.Get(siteId);
			savedModel.AgentsOutOfAdherence.Should().Be.EqualTo(5);
		}

		[Test]
		public void ShouldBeNullIfNotExists()
		{
			var model = Target.Get(Guid.NewGuid());

			model.Should().Be.Null();
		}

		[Test]
		public void ShouldReadAllSitesFromBusinessUnit()
		{
			var site1 = Guid.NewGuid();
			var site2 = Guid.NewGuid();
			var site3 = Guid.NewGuid();
			var buId1 = Guid.NewGuid();
			var model1 = new SiteAdherenceReadModel() { AgentsOutOfAdherence = 21, SiteId = site1, BusinessUnitId = buId1};
			var model2 = new SiteAdherenceReadModel() { AgentsOutOfAdherence = 8, SiteId = site2, BusinessUnitId = buId1 };
			var model3 = new SiteAdherenceReadModel() { AgentsOutOfAdherence = 8, SiteId = site3, BusinessUnitId = Guid.NewGuid() };

			Target.Persist(model1);
			Target.Persist(model2);
			Target.Persist(model3);

			var models = Target.GetAll(buId1);

			models.Single(m => m.SiteId == site1).AgentsOutOfAdherence.Should().Be.EqualTo(21);
			models.Single(m => m.SiteId == site2).AgentsOutOfAdherence.Should().Be.EqualTo(8);
			models.Count().Should().Be(2);

		}
	}
}