using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence.Domain.Service.AgentStateReadModel
{
	[TestFixture]
	[RtaTest]
	public class UpdateAgentStateReadModelTest
	{
		public FakeDatabase Database;
		public FakeRtaStateGroupRepository StateGroups;
		public Ccc.Domain.RealTimeAdherence.Domain.Service.Rta Target;
		public MutableNow Now;
		public FakeAgentStateReadModelPersister ReadModels;

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

			ReadModels.Models.Last()
				.StateName.Should().Be("phone");
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

			ReadModels.Models.Last()
				.Should().Not.Be.Null();
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

			ReadModels.Models.Last()
				.ReceivedTime.Should().Be("2014-10-20 10:00".Utc());
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

			ReadModels.Models.Last()
				.Activity.Should().Be("Phone");
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

			ReadModels.Models.Single(x => x.PersonId == person)
				.NextActivity.Should().Be("Phone");
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

			ReadModels.Models.Single(x => x.PersonId == person)
				.NextActivityStartTime.Should().Be("2016-05-30 14:30".Utc());
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

			ReadModels.Models.Single(x => x.PersonId == personId)
				.RuleName.Should().Be("rule");
		}

		[Test]
		public void ShouldPersistWithState()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 09:00", "2014-10-20 11:00")
				.WithStateGroup(null, "my state")
				.WithStateCode("statecode")
				.WithMappedRule("statecode", activityId);
			Now.Is("2014-10-20 10:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			ReadModels.Models.Single(x => x.PersonId == personId)
				.StateName.Should().Be("my state");
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

			ReadModels.Models.Single(x => x.PersonId == personId)
				.StateStartTime.Should().Be.EqualTo("2014-10-20 10:01".Utc());
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

			ReadModels.Models.Last()
				.StateGroupId.Should().Be(stateGroupId);
		}
	}
}