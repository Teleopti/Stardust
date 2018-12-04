using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.Domain.Service;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service.AgentStateReadModel.StartTime
{
	[RtaTest]
	[TestFixture]
	public class RuleStartTimeTest
	{
		public FakeDatabase Database;
		public MutableNow Now;
		public Rta Target;
		public FakeAgentStateReadModelPersister ReadModels;

		[Test]
		public void ShouldHaveRuleStartTimeWhenHavingAdherence()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2015-12-10 8:00", "2015-12-10 9:00")
				.WithMappedRule("phone", phone, 0, Adherence.Configuration.Adherence.In)
				.WithAlarm(TimeSpan.FromMinutes(5));

			Now.Is("2015-12-10 8:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			ReadModels.Models.Single(x => x.PersonId == personId)
				.RuleStartTime.Should().Be("2015-12-10 8:00".Utc());
		}

		[Test]
		public void ShouldNotChangeRuleStartTimeWhenStillInSameRule()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2015-12-10 8:00", "2015-12-10 9:00")
				.WithMappedRule("phone", phone, 0, Adherence.Configuration.Adherence.In)
				.WithAlarm(TimeSpan.FromMinutes(5));

			Now.Is("2015-12-10 8:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Now.Is("2015-12-10 8:30");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			ReadModels.Models.Single(x => x.PersonId == personId)
				.RuleStartTime.Should().Be("2015-12-10 8:00".Utc());
		}

		[Test]
		public void ShouldUpdateRuleStartTimeWhenChangingRule()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2015-12-10 8:00", "2015-12-10 9:00")
				.WithMappedRule("phone", phone, 0, Adherence.Configuration.Adherence.In)
				.WithAlarm(TimeSpan.FromMinutes(5))
				.WithMappedRule("break", phone, -1, Adherence.Configuration.Adherence.Out)
				.WithAlarm(TimeSpan.FromMinutes(5));

			Now.Is("2015-12-10 8:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Now.Is("2015-12-10 8:30");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "break"
			});

			ReadModels.Models.Single(x => x.PersonId == personId)
				.RuleStartTime.Should().Be("2015-12-10 8:30".Utc());
		}

		[Test]
		public void ShouldUpdateRuleStartTimeWhenChangingRuleWithSameAdherence()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2015-12-10 8:00", "2015-12-10 9:00")
				.WithMappedRule("phone", phone, 0, Adherence.Configuration.Adherence.In)
				.WithAlarm(TimeSpan.FromMinutes(5))
				.WithMappedRule("ACW", phone, 0, Adherence.Configuration.Adherence.In)
				.WithAlarm(TimeSpan.FromMinutes(5));

			Now.Is("2015-12-10 8:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Now.Is("2015-12-10 8:30");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "ACW"
			});

			ReadModels.Models.Single(x => x.PersonId == personId)
				.RuleStartTime.Should().Be("2015-12-10 8:30".Utc());
		}
	}
}