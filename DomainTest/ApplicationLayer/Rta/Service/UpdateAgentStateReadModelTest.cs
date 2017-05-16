using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	public class UpdateAgentStateReadModelTest
	{
		public FakeRtaDatabase Database;
		public FakeRtaStateGroupRepository StateGroups;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;
		public MutableNow Now;

		[Test]
		public void ShouldPersistReadModel()
		{
			Database
				.WithAgent("usercode")
				.WithStateCode("phone");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Database.PersistedReadModel.StateName.Should().Be("phone");
		}

		[Test]
		public void ShouldPersistWhenNotifiedOfPossibleScheduleChange()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				;
			Now.Is("2014-10-20 10:00");

			Target.CheckForActivityChanges(Database.TenantName(), personId);

			Database.PersistedReadModel.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPersistWithReceivedSystemTime()
		{
			Database
				.WithAgent("usercode")
				;
			Now.Is("2014-10-20 10:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			Database.PersistedReadModel.ReceivedTime.Should().Be("2014-10-20 10:00".Utc());
		}

		[Test]
		public void ShouldNotPersistAssociation()
		{
			Database
				.WithAgent("user", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

			Target.ProcessState(new StateForTest
			{
				UserCode = "user",
				StateCode = "state"
			});

			Database.PersistedReadModel.TeamId.Should().Be(null);
			Database.PersistedReadModel.SiteId.Should().Be(null);
			Database.PersistedReadModel.BusinessUnitId.Should().Be(null);
		}
		
		[Test]
		public void ShouldUpdateCurrentActivityChanges()
		{
			var person = Guid.NewGuid();
			Database
				.WithAgent("user", person);
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
				.WithAgent("user", person);
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
				.WithAgent("user", person);
			Now.Is("2016-05-30 14:00");
			Database.WithSchedule(person, phone, "Phone", "2016-05-30 15:00", "2016-05-30 16:00");

			Target.CheckForActivityChanges(Database.TenantName());
			Database
				.ClearAssignments(person)
				.WithSchedule(person, phone, "Phone", "2016-05-30 14:30", "2016-05-30 16:00");
			Now.Is("2016-05-30 14:01");
			Target.CheckForActivityChanges(Database.TenantName());

			Database.PersistedReadModel.NextActivityStartTime.Should().Be("2016-05-30 14:30".Utc());
		}

		[Test]
		public void ShouldPersistWithAlarm()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var alarmId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.WithMappedRule("statecode", activityId, alarmId, "rule")
				;
			Now.Is("2014-10-20 10:00");

			Target.ProcessState(new StateForTest
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
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 09:00", "2014-10-20 11:00")
				.WithMappedRule("statecode", activityId, "my state");
			Now.Is("2014-10-20 10:00");

			Target.ProcessState(new StateForTest
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
				.WithAgent("usercode", personId)
				.WithMappedRule("statecode", activityId, 0)
				.WithSchedule(personId, activityId, "2014-10-20 9:00", "2014-10-20 11:00");
			Now.Is("2014-10-20 10:01");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			Database.PersistedReadModel.StateStartTime.Should().Be.EqualTo("2014-10-20 10:01".Utc());
		}

		[Test]
		public void ShouldPersistStateGroupId()
		{
			Database
				.WithAgent("usercode")
				.WithMappedRule("phone");
			var stateGroupId = StateGroups.LoadAll().Single().Id;

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Database.PersistedReadModel.StateGroupId.Should().Be(stateGroupId);
		}
	}
}