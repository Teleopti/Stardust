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
	public class StateStreamSynchronizerTeamAdherenceTest
	{
		public FakeRtaDatabase Database;
		public IStateStreamSynchronizer Target;
		public FakeTeamAdherencePersister Model;

		[Test]
		public void ShouldInitializeTeamAdherence()
		{
			var teamIdA = Guid.NewGuid();
			var teamIdB = Guid.NewGuid();
			var personIdA1 = Guid.NewGuid();
			var personIdA2 = Guid.NewGuid();
			var personIdA3 = Guid.NewGuid();
			var personIdB1 = Guid.NewGuid();
			var personIdB2 = Guid.NewGuid();
			var personIdB3 = Guid.NewGuid();

			Database
				.WithExistingState(personIdA1, 1)
				.WithUser("", personIdA1, null, teamIdA, null)
				.WithExistingState(personIdA2, 1)
				.WithUser("", personIdA2, null, teamIdA, null)
				.WithExistingState(personIdA3, 0)
				.WithUser("", personIdA3, null, teamIdA, null)
				.WithExistingState(personIdB1, -1)
				.WithUser("", personIdB1, null, teamIdB, null)
				.WithExistingState(personIdB2, 0)
				.WithUser("", personIdB2, null, teamIdB, null)
				.WithExistingState(personIdB3, 0)
				.WithUser("", personIdB3, null, teamIdB, null)
				;

			Target.Initialize();

			Model.Get(teamIdA).AgentsOutOfAdherence.Should().Be(2);
			Model.Get(teamIdB).AgentsOutOfAdherence.Should().Be(1);
		}

		[Test]
		public void ShouldNotReinitializeTeamAdherenceOnInitialize()
		{
			var existingTeam = Guid.NewGuid();
			var stateTeam = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Model.Persist(new TeamAdherenceReadModel
			{
				AgentsOutOfAdherence = 3,
				TeamId = existingTeam
			});
			Database
				.WithExistingState(personId, 1)
				.WithUser("", personId, null, stateTeam, null);

			Target.Initialize();

			Model.Get(existingTeam).AgentsOutOfAdherence.Should().Be(3);
			Model.Get(stateTeam).Should().Be.Null();
		}

		[Test]
		public void ShouldReinitializeTeamAdherenceOnSync()
		{
			var teamId1 = Guid.NewGuid();
			var teamId2 = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Model.Persist(new TeamAdherenceReadModel
			{
				AgentsOutOfAdherence = 3,
				TeamId = teamId1
			});
			Model.Persist(new TeamAdherenceReadModel
			{
				AgentsOutOfAdherence = 3,
				TeamId = teamId2
			});
			Database
				.WithExistingState(personId, 1)
				.WithUser("", personId, null, teamId1, null);

			Target.Sync();

			Model.Get(teamId1).AgentsOutOfAdherence.Should().Be(1);
			Model.Get(teamId2).Should().Be.Null();
		}

	}
}