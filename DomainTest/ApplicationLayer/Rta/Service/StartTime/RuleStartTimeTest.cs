using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service.StartTime
{
	[RtaTest]
	[TestFixture]
	public class RuleStartTimeTest
	{
		public FakeRtaDatabase Database;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldHaveRuleStartTimeWhenHavingAdherence()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2015-12-10 8:00", "2015-12-10 9:00")
				.WithMappedRule("phone", phone, 0, Adherence.In)
				.WithAlarm(TimeSpan.FromMinutes(5));

			Now.Is("2015-12-10 8:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Database.PersistedReadModel
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
				.WithMappedRule("phone", phone, 0, Adherence.In)
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

			Database.PersistedReadModel
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
				.WithMappedRule("phone", phone, 0, Adherence.In)
				.WithAlarm(TimeSpan.FromMinutes(5))
				.WithMappedRule("break", phone, -1, Adherence.Out)
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

			Database.PersistedReadModel
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
				.WithMappedRule("phone", phone, 0, Adherence.In)
				.WithAlarm(TimeSpan.FromMinutes(5))
				.WithMappedRule("ACW", phone, 0, Adherence.In)
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

			Database.PersistedReadModel
				.RuleStartTime.Should().Be("2015-12-10 8:30".Utc());
		}

	}
}
