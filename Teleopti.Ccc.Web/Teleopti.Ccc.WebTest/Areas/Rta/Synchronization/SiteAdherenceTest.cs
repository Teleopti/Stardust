using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server;

namespace Teleopti.Ccc.WebTest.Areas.Rta.Synchronization
{
	[RtaTest]
	[Toggle(Toggles.RTA_SeeAdherenceDetailsForOneAgent_31285)]
	[Toggle(Toggles.RTA_SeePercentageAdherenceForOneAgent_30783)]
	[Toggle(Toggles.RTA_NoBroker_31237)]
	[TestFixture]
	public class SiteAdherenceTest
	{
		public FakeRtaDatabase Database;
		public IStateStreamSynchronizer Target;
		public FakeSiteOutOfAdherenceReadModelPersister Model;

		[Test]
		public void ShouldInitializeSiteAdherence()
		{
			var siteId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Database
				.WithExistingState(personId, 1)
				.WithUser("", personId, null, null, siteId)
				;

			Target.Initialize();

			Model.Get(siteId).Count.Should().Be(1);
		}

		[Test]
		public void ShouldNotReinitializeSiteAdherenceOnInitialize()
		{
			var existingSite = Guid.NewGuid();
			var stateSite = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Model.Persist(new SiteOutOfAdherenceReadModel
			{
				Count = 3,
				SiteId = existingSite
			});
			Database
				.WithExistingState(personId, 1)
				.WithUser("", personId, null, null, stateSite);

			Target.Initialize();

			Model.Get(existingSite).Count.Should().Be(3);
			Model.Get(stateSite).Should().Be.Null();
		}

		[Test]
		public void ShouldReinitializeSiteAdherenceOnSync()
		{
			var siteId1 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Model.Persist(new SiteOutOfAdherenceReadModel
			{
				Count = 3,
				SiteId = siteId1
			});
			Model.Persist(new SiteOutOfAdherenceReadModel
			{
				Count = 3,
				SiteId = siteId2
			});
			Database
				.WithExistingState(personId, 1)
				.WithUser("", personId, null, null, siteId1);

			Target.Sync();

			Model.Get(siteId1).Count.Should().Be(1);
			Model.Get(siteId2).Should().Be.Null();
		}
	}
}