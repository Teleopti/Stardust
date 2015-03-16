using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	public class UpdateAgentStateReadModelTest
	{
		public FakeRtaDatabase Database;
		public IRta Target;

		[Test]
		public void ShouldPersistReadModel()
		{
			var state = new ExternalUserStateForTest();
			Database.WithDataFromState(state);

			Target.SaveState(state);

			Database.PersistedAgentStateReadModel.StateCode.Should().Be(state.StateCode);
		}

		[Test]
		public void ShouldCutStateCodeIfToLong()
		{
			var state = new ExternalUserStateForTest();
			Database.WithDataFromState(state);

			state.StateCode = "a really really really really looooooooong statecode that should be trimmed somehow for whatever reason";
			Target.SaveState(state);

			Database.PersistedAgentStateReadModel.StateCode.Should().Be(state.StateCode.Substring(0, 25));
		}

		[Test]
		public void ShouldPersistStateCodeToLoggedOutIfNotLoggedIn()
		{
			var state = new ExternalUserStateForTest {IsLoggedOn = false};
			Database.WithDataFromState(state);

			state.IsLoggedOn = false;
			Target.SaveState(state);

			Database.PersistedAgentStateReadModel.StateCode.Should().Be(Domain.ApplicationLayer.Rta.Service.Rta.LogOutStateCode);
		}

		[Test]
		public void ShouldPersistWhenScheduleIsUpdated()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var state = new ExternalUserStateForTest();
			Database
				.WithSource(state.SourceId)
				.WithUser(state.UserCode, personId, businessUnitId);

			Target.CheckForActivityChange(personId, businessUnitId);

			Database.PersistedAgentStateReadModel.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldPersistNextStartWithNullWhenNoActivity()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var state = new ExternalUserStateForTest();
			Database
				.WithSource(state.SourceId)
				.WithUser(state.UserCode, personId, businessUnitId);

			Target.CheckForActivityChange(personId, businessUnitId);

			Database.PersistedAgentStateReadModel.NextStart.Should().Be(null);
		}

		[Test]
		public void ShouldPersistTeamId()
		{
			var teamId = Guid.NewGuid();
			Database
				.WithUser("user", Guid.NewGuid(), Guid.NewGuid(), teamId, null);

			Target.SaveState(new ExternalUserStateForTest
			{
				StateCode = "state",
				UserCode = "user"
			});

			Database.PersistedAgentStateReadModel.TeamId.Should().Be(teamId);
		}

		[Test]
		public void ShouldPersistSiteId()
		{
			var siteId = Guid.NewGuid();
			Database
				.WithUser("user", Guid.NewGuid(), Guid.NewGuid(), null, siteId);

			Target.SaveState(new ExternalUserStateForTest
			{
				StateCode = "state",
				UserCode = "user"
			});

			Database.PersistedAgentStateReadModel.SiteId.Should().Be(siteId);
		}

	}
}