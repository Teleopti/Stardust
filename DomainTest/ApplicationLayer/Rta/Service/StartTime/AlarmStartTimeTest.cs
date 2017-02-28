﻿using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service.StartTime
{
	[RtaTest]
	[TestFixture]
	public class AlarmStartTimeTest
	{
		public FakeRtaDatabase Database;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldHaveAlarmStartTimeWhenEnteringRule()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2015-12-10 8:00", "2015-12-10 9:00")
				.WithRule("phone", phone, 0, Adherence.In)
				.WithAlarm(TimeSpan.FromMinutes(5));
			Now.Is("2015-12-10 8:00");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Database.PersistedReadModel.AlarmStartTime.Should().Be("2015-12-10 8:05".Utc());
		}

		[Test]
		public void ShouldNotHaveAlarmStartTimeWhenNotInRule()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2015-12-10 8:00", "2015-12-10 9:00")
				.WithRule("phone", phone, (Guid?)null);
			Now.Is("2015-12-10 8:00");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Database.PersistedReadModel.AlarmStartTime.Should().Be(null);
		}

		[Test]
		public void ShouldNotUpdateAlarmStartTimeWhenStillInSameAlarm()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var rule = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2015-12-10 8:00", "2015-12-10 9:00")
				.WithRule(rule, "phone", phone)
				.WithAlarm("5".Minutes())
				.WithRule(rule, "ACW", phone)
				.WithAlarm("5".Minutes())
				;
			Now.Is("2015-12-10 8:00");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Now.Is("2015-12-10 8:10");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "ACW"
			});

			Database.PersistedReadModel.AlarmStartTime.Should().Be("2015-12-10 8:05".Utc());
		}

		[Test]
		public void ShouldNotResetAlarmTimeWhenTransitioningBetweenInAlarmRules()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2015-12-10 8:00", "2015-12-10 9:00")
				.WithRule("phone", phone)
				.WithAlarm("5".Minutes())
				.WithRule("ACW", phone)
				.WithAlarm("0".Minutes())
				;
			Now.Is("2015-12-10 8:00");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Now.Is("2015-12-10 8:10");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "ACW"
			});

			Database.PersistedReadModel.AlarmStartTime.Should().Be("2015-12-10 8:05".Utc());
		}


		[Test]
		public void ShouldNotCountThresholdWhenTransitioningBetweenInAlarmRules()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2015-12-10 8:00", "2015-12-10 9:00")
				.WithRule("phone", phone)
				.WithAlarm("5".Minutes())
				.WithRule("ACW", phone)
				.WithAlarm("5".Minutes())
				;
			Now.Is("2015-12-10 8:00");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Now.Is("2015-12-10 8:10");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "ACW"
			});

			Database.PersistedReadModel.AlarmStartTime.Should().Be("2015-12-10 8:05".Utc());
		}
	}
}