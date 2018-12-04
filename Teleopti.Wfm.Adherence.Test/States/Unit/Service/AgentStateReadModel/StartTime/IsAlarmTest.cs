using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.Domain.Service;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service.AgentStateReadModel.StartTime
{
	[RtaTest]
	[TestFixture]
	public class IsAlarmTest
	{
		public FakeDatabase Database;
		public MutableNow Now;
		public Rta Target;
		public FakeAgentStateReadModelPersister ReadModels;

		[Test]
		public void ShouldBeInAlarmIfEnteredRuleIsAlarm()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2015-12-15 8:00", "2015-12-15 9:00")
				.WithMappedRule("phone", phone, 0, Adherence.Configuration.Adherence.In)
				.WithAlarm(TimeSpan.FromMinutes(5));
			Now.Is("2015-12-15 8:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			ReadModels.Models.Single(x => x.PersonId == personId)
				.IsRuleAlarm.Should().Be(true);
		}

		[Test]
		public void ShouldNotBeInAlarmIfEnteredRuleIsNotAlarm()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2015-12-15 8:00", "2015-12-15 9:00")
				.WithMappedRule("phone", phone, 0, Adherence.Configuration.Adherence.In);
			Now.Is("2015-12-15 8:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			ReadModels.Models.Single(x => x.PersonId == personId)
				.IsRuleAlarm.Should().Be(false);
		}
	}
}