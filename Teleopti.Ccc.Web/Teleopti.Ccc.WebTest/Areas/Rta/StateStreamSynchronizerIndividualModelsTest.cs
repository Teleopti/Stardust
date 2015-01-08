using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	[RtaTest]
	[Toggle(Toggles.RTA_SeeAdherenceDetailsForOneAgent_31285)]
	[Toggle(Toggles.RTA_SeePercentageAdherenceForOneAgent_30783)]
	[Toggle(Toggles.RTA_NoBroker_31237)]
	[TestFixture]
	public class StateStreamSynchronizerIndividualModelsTest
	{
		public FakeRtaDatabase Database;
		public IStateStreamSynchronizer Target;
		public FakeSiteAdherencePersister SiteAdherence;
		public FakeTeamAdherencePersister TeamAdherence;

		[Test]
		public void ShouldInitializeModelsWithoutData()
		{
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			TeamAdherence.Persist(new TeamAdherenceReadModel
			{
				TeamId = teamId,
				AgentsOutOfAdherence = 3
			});
			Database
				.WithExistingState(personId, 1)
				.WithUser("", personId, null, teamId, siteId)
				;

			Target.Initialize();

			TeamAdherence.Get(teamId).AgentsOutOfAdherence.Should().Be(3);
			SiteAdherence.Get(siteId).AgentsOutOfAdherence.Should().Be(1);
		}

	}
}