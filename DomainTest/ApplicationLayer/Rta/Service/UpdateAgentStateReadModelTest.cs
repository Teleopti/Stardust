using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	public class UpdateAgentStateReadModelTest
	{
		public FakeRtaDatabase Database;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldPersistReadModel()
		{
			Database
				.WithUser("usercode");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Database.StoredState.StateCode.Should().Be("phone");
		}

		[Test]
		public void ShouldCutStateCodeIfToLong()
		{
			Database
				.WithUser("usercode");

			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "a really really really really looooooooong statecode that should be trimmed somehow for whatever reason"
			};
			Target.SaveState(state);

			Database.StoredState.StateCode.Should().Be(state.StateCode.Substring(0, 25));
		}

		[Test]
		public void ShouldPersistStateCodeToLoggedOutIfNotLoggedIn()
		{
			Database
				.WithUser("usercode");
			
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				IsLoggedOn = false
			});

			Database.StoredState.StateCode.Should().Be(Domain.ApplicationLayer.Rta.Service.Rta.LogOutStateCode);
		}

		[Test]
		// no schedule == schedule is updated? :-)
		public void ShouldPersistWhenScheduleIsUpdated()
		{
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId);

			Target.CheckForActivityChanges(Database.TenantName(), personId);

			Database.StoredState.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldPersistNextStartWithNullWhenNoActivity()
		{
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId);

			Target.CheckForActivityChanges(Database.TenantName(), personId);

			Database.PersistedReadModel.NextActivityStartTime.Should().Be(null);
		}

		[Test]
		public void ShouldPersistTeamId()
		{
			var teamId = Guid.NewGuid();
			Database
				.WithUser("user", Guid.NewGuid(), Guid.NewGuid(), teamId, null);

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "user",
				StateCode = "state"
			});

			Database.PersistedReadModel.TeamId.Should().Be(teamId);
		}

		[Test]
		public void ShouldPersistSiteId()
		{
			var siteId = Guid.NewGuid();
			Database
				.WithUser("user", Guid.NewGuid(), Guid.NewGuid(), null, siteId);

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "user",
				StateCode = "state",
			});

			Database.PersistedReadModel.SiteId.Should().Be(siteId);
		}

	}
}