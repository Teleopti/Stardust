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

			Database.PersistedReadModel.StateCode.Should().Be("phone");
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