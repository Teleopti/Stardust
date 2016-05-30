using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	public class UpdateAgentStateReadModelTest
	{
		public FakeRtaDatabase Database;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;
		public MutableNow Now;

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

		[Test]
		public void ShouldUpdateCurrentActivityChanges()
		{
			var person = Guid.NewGuid();
			Database
				.WithUser("user", person);
			Now.Is("2016-05-30 14:00");

			Target.CheckForActivityChanges(Database.TenantName());
			Database.WithSchedule(person, Guid.NewGuid(), "Phone", "2016-05-30 14:00", "2016-05-30 15:00");
			Now.Is("2016-05-30 14:01");
			Target.CheckForActivityChanges(Database.TenantName());

			Database.PersistedReadModel.Activity.Should().Be("Phone");
		}

		[Test]
		public void ShouldUpdateNextActivityChanges()
		{
			var person = Guid.NewGuid();
			Database
				.WithUser("user", person);
			Now.Is("2016-05-30 14:00");

			Target.CheckForActivityChanges(Database.TenantName());
			Database.WithSchedule(person, Guid.NewGuid(), "Phone", "2016-05-30 15:00", "2016-05-30 16:00");
			Now.Is("2016-05-30 14:01");
			Target.CheckForActivityChanges(Database.TenantName());

			Database.PersistedReadModel.NextActivity.Should().Be("Phone");
		}

		[Test]
		public void ShouldUpdateNextActivityStartTimeChanges()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("user", person);
			Now.Is("2016-05-30 14:00");
			Database.WithSchedule(person, phone, "Phone", "2016-05-30 15:00", "2016-05-30 16:00");

			Target.CheckForActivityChanges(Database.TenantName());
			Database
				.ClearSchedule(person)
				.WithSchedule(person, phone, "Phone", "2016-05-30 14:30", "2016-05-30 16:00");
			Now.Is("2016-05-30 14:01");
			Target.CheckForActivityChanges(Database.TenantName());

			Database.PersistedReadModel.NextActivityStartTime.Should().Be("2016-05-30 14:30".Utc());
		}
	}
}