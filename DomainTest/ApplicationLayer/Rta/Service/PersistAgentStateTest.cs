using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[RtaTest]
	[TestFixture]
	public class PersistAgentStateTest
	{
		public FakeRtaDatabase Database;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldPersist()
		{
			Database
				.WithUser("usercode")
				;
			Now.Is("2014-10-20 10:00");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			Database.PersistedReadModel.Should().Not.Be.Null();
		}

		[Test, Ignore]
		public void ShouldNotPersistWhenWrongDataSource()
		{
			Assert.Fail();
		}

		[Test, Ignore]
		public void ShouldNotPersisthenWrongPerson()
		{
			Assert.Fail();
		}

		[Test]
		public void ShouldPersistWhenNotifiedOfPossibleScheduleChange()
		{
			var personId = Guid.NewGuid();
			Database 
				.WithUser("usercode", personId)
				;
			Now.Is("2014-10-20 10:00");

			Target.CheckForActivityChanges(Database.TenantName(), personId);

			Database.PersistedReadModel.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPersistWithReceivedSystemTime()
		{
			Database
				.WithUser("usercode")
				;
			Now.Is("2014-10-20 10:00");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			Database.PersistedReadModel.ReceivedTime.Should().Be("2014-10-20 10:00".Utc());
		}

		[Test]
		public void ShouldPersistWithCurrentActivity()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "Phone", "2014-10-20 10:00", "2014-10-20 11:00")
				;
			Now.Is("2014-10-20 10:00");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			Database.PersistedReadModel.Activity.Should().Be("Phone");
		}

		[Test]
		public void ShouldNotPersistWithCurrentActivityIfNoSchedule()
		{
			Database
				.WithUser("usercode")
				;
			Now.Is("2014-10-20 10:00");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			Database.PersistedReadModel.Activity.Should().Be(null);
		}

		[Test]
		public void ShouldNotPersistWithCurrentActivityIfFutureSchedule()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			};
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 11:00", "2014-10-20 12:00")
				;
			Now.Is("2014-10-20 10:00");

			Target.SaveState(state);

			Database.PersistedReadModel.Activity.Should().Be(null);
		}

		[Test]
		public void ShouldPersistWithNextActivity()
		{
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, Guid.NewGuid(), "Lunch", "2014-10-20 10:00", "2014-10-20 11:00")
				.WithSchedule(personId, Guid.NewGuid(), "Phone", "2014-10-20 11:00", "2014-10-20 11:00")
				;
			Now.Is("2014-10-20 10:00");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			Database.PersistedReadModel.NextActivity.Should().Be("Phone");
		}

		[Test]
		public void ShouldNotPersistWithNextActivityFromNextShift()
		{
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, Guid.NewGuid(), "Phone", "2014-10-20 10:00", "2014-10-20 11:00")
				.WithSchedule(personId, Guid.NewGuid(), "Phone", "2014-10-21 10:00", "2014-10-21 11:00")
				;
			Now.Is("2014-10-20 10:00");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			Database.PersistedReadModel.NextActivity.Should().Be(null);
		}

		[Test]
		public void ShouldPersistWithNextActivityFromFutureShift()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "Phone", "2014-10-20 11:00", "2014-10-20 12:00")
				;
			Now.Is("2014-10-20 10:00");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			Database.PersistedReadModel.NextActivity.Should().Be("Phone");
		}


		[Test]
		public void ShouldPersistWithAlarm()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var alarmId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.WithRule("statecode", activityId, alarmId, "rule")
				;
			Now.Is("2014-10-20 10:00");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			Database.PersistedReadModel.RuleName.Should().Be("rule");
		}

		[Test]
		public void ShouldPersistWithState()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 09:00", "2014-10-20 11:00")
				.WithRule("statecode", activityId, "my state");
			Now.Is("2014-10-20 10:00");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});
			
			Database.PersistedReadModel.StateName.Should().Be("my state");
		}

		[Test]
		public void ShouldPersistWithStateStartTimeFromSystemTime()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithRule("statecode", activityId, 0)
				.WithSchedule(personId, activityId, "2014-10-20 9:00", "2014-10-20 11:00");
			Now.Is("2014-10-20 10:01");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});
			
			Database.PersistedReadModel.StateStartTime.Should().Be.EqualTo("2014-10-20 10:01".Utc());
		}
	}
}
