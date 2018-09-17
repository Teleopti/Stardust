﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Wfm.Adherence.Test.Domain.Service.AgentStateReadModel.StartTime
{
	[RtaTest]
	[TestFixture]
	public class ProperAlarmStartTimeTest
	{
		public FakeDatabase Database;
		public MutableNow Now;
		public Ccc.Domain.RealTimeAdherence.Domain.Service.Rta Target;
		public FakeAgentStateReadModelPersister ReadModels;

		[Test]
		public void ShouldHaveAlarmStartTimeWhenEnteringAlarm()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2015-12-10 8:00", "2015-12-10 9:00")
				.WithMappedRule("phone", phone, 0, Ccc.Domain.InterfaceLegacy.Domain.Adherence.In)
				.WithAlarm(TimeSpan.FromMinutes(5));
			Now.Is("2015-12-10 8:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			ReadModels.Models.Single(x => x.PersonId == personId)
				.AlarmStartTime.Should().Be("2015-12-10 8:05".Utc());
		}

		[Test]
		public void ShouldNotHaveAlarmStartTimeWhenNotInAlarm()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2015-12-10 8:00", "2015-12-10 9:00")
				.WithMappedRule("phone", phone, 0, Ccc.Domain.InterfaceLegacy.Domain.Adherence.In);
			Now.Is("2015-12-10 8:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			ReadModels.Models.Single(x => x.PersonId == personId)
				.AlarmStartTime.Should().Be(null);
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
				.WithMappedRule(rule, "phone", phone)
				.WithAlarm("5".Minutes())
				.WithMappedRule(rule, "ACW", phone)
				.WithAlarm("5".Minutes())
				;
			Now.Is("2015-12-10 8:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Now.Is("2015-12-10 8:10");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "ACW"
			});

			ReadModels.Models.Single(x => x.PersonId == personId)
				.AlarmStartTime.Should().Be("2015-12-10 8:05".Utc());
		}
	}
}