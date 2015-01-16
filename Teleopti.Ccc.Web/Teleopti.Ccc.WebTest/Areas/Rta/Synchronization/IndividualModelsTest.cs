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
	public class IndividualModelsTest
	{
		public FakeRtaDatabase Database;
		public IStateStreamSynchronizer Target;
		public FakeSiteOutOfOutOfAdherenceReadModelReadModelPersister SiteOutOfOutOfAdherenceReadModelReadModel;
		public FakeTeamOutOfAdherenceReadModelPersister TeamOutOfAdherenceReadModel;

		[Test]
		public void ShouldInitializeModelsWithoutData()
		{
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			TeamOutOfAdherenceReadModel.Persist(new TeamOutOfAdherenceReadModel
			{
				TeamId = teamId,
				Count = 3
			});
			Database
				.WithExistingState(personId, 1)
				.WithUser("", personId, null, teamId, siteId)
				;

			Target.Initialize();

			TeamOutOfAdherenceReadModel.Get(teamId).Count.Should().Be(3);
			SiteOutOfOutOfAdherenceReadModelReadModel.Get(siteId).Count.Should().Be(1);
		}

	}
}