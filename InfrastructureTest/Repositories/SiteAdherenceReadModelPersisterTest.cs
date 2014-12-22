using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, Category("LongRunning")]
	public class SiteAdherenceReadModelPersisterTest : IReadModelReadWriteTest
	{
		public ISiteAdherencePersister Target { get; set; }
		[Test]
		public void ShouldSaveReadModelForSite()
		{
			var siteId = Guid.NewGuid();
			var model = new SiteAdherenceReadModel() { AgentsOutOfAdherence = 21, SiteId = siteId };

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
		public void AgentsOutOfAdherenceShouldBeZeroIfReadModelDoesNotExist()
		{
			var siteId = Guid.NewGuid();
			var model = Target.Get(siteId);

			model.AgentsOutOfAdherence.Should().Be.EqualTo(0);
			model.SiteId.Should().Be.EqualTo(siteId);
		}
	}
}