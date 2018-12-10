using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service.AgentStateReadModel
{
	[TestFixture]
	[RtaTest]
	public class UpdateAgentStateReadModelTest
	{
		public FakeDatabase Database;
		public FakeRtaStateGroupRepository StateGroups;
		public Rta Target;
		public MutableNow Now;
		public FakeAgentStateReadModelPersister ReadModels;

		[Test]
		public void ShouldPersistReadModel()
		{
			var person = Guid.NewGuid();
			Database
				.WithAgent("usercode", person)
				.WithStateCode("phone");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			ReadModels.Models.Single(x => x.PersonId == person)
				.StateName.Should().Be("phone");
		}

		[Test]
		public void ShouldPersistWhenNotifiedOfPossibleScheduleChange()
		{
			var person = Guid.NewGuid();
			Database
				.WithAgent("usercode", person)
				;
			Now.Is("2014-10-20 10:00");

			Target.CheckForActivityChanges(Database.TenantName(), person);

			ReadModels.Models.Single(x => x.PersonId == person)
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPersistWithReceivedSystemTime()
		{
			var person = Guid.NewGuid();
			Database
				.WithAgent("usercode", person)
				;
			Now.Is("2014-10-20 10:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			ReadModels.Models.Single(x => x.PersonId == person)
				.ReceivedTime.Should().Be("2014-10-20 10:00".Utc());
		}

		[Test]
		public void ShouldUpdateCurrentActivityChanges()
		{
			var person = Guid.NewGuid();
			Database
				.WithAgent("usercode", person);
			Now.Is("2016-05-30 14:00");

			Target.CheckForActivityChanges(Database.TenantName());
			Database.WithSchedule(person, Guid.NewGuid(), "Phone", "2016-05-30 14:00", "2016-05-30 15:00");
			Now.Is("2016-05-30 14:01");
			Target.CheckForActivityChanges(Database.TenantName());

			ReadModels.Models.Single(x => x.PersonId == person)
				.Activity.Should().Be("Phone");
		}

		[Test]
		public void ShouldUpdateNextActivityChanges()
		{
			var person = Guid.NewGuid();
			Database
				.WithAgent("usercode", person);
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
				.WithAgent("usercode", person);
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
			var person = Guid.NewGuid();
			var activity = Guid.NewGuid();
			var alarm = Guid.NewGuid();
			Database
				.WithAgent("usercode", person)
				.WithSchedule(person, activity, "2014-10-20 10:00", "2014-10-20 11:00")
				.WithMappedRule("statecode", activity, alarm, "rule")
				;
			Now.Is("2014-10-20 10:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			ReadModels.Models.Single(x => x.PersonId == person)
				.RuleName.Should().Be("rule");
		}

		[Test]
		public void ShouldPersistWithState()
		{
			var person = Guid.NewGuid();
			var activity = Guid.NewGuid();
			Database
				.WithAgent("usercode", person)
				.WithSchedule(person, activity, "2014-10-20 09:00", "2014-10-20 11:00")
				.WithStateGroup(null, "my state")
				.WithStateCode("statecode")
				.WithMappedRule("statecode", activity);
			Now.Is("2014-10-20 10:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			ReadModels.Models.Single(x => x.PersonId == person)
				.StateName.Should().Be("my state");
		}

		[Test]
		public void ShouldPersistWithStateStartTimeFromSystemTime()
		{
			var person = Guid.NewGuid();
			var activity = Guid.NewGuid();
			Database
				.WithAgent("usercode", person)
				.WithMappedRule("statecode", activity, 0)
				.WithSchedule(person, activity, "2014-10-20 9:00", "2014-10-20 11:00");
			Now.Is("2014-10-20 10:01");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			ReadModels.Models.Single(x => x.PersonId == person)
				.StateStartTime.Should().Be.EqualTo("2014-10-20 10:01".Utc());
		}

		[Test]
		public void ShouldPersistStateGroupId()
		{
			var person = Guid.NewGuid();
			Database
				.WithAgent("usercode", person)
				.WithMappedRule("phone");
			var stateGroupId = StateGroups.LoadAll().Single().Id;

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			ReadModels.Models.Single(x => x.PersonId == person)
				.StateGroupId.Should().Be(stateGroupId);
		}
	}
}