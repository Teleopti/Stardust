using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service.StartTime
{
	[RtaTest]
	[TestFixture]
	public class IsAlarmTest
	{
		public FakeRtaDatabase Database;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;
		
		[Test]
		public void ShouldBeInAlarmIfEnteredRuleIsAlarm()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2015-12-15 8:00", "2015-12-15 9:00")
				.WithMappedRule("phone", phone, 0, Adherence.In)
				.WithAlarm(TimeSpan.FromMinutes(5));
			Now.Is("2015-12-15 8:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Database.PersistedReadModel.IsRuleAlarm.Should().Be(true);
		}

		[Test]
		public void ShouldNotBeInAlarmIfEnteredRuleIsNotAlarm()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2015-12-15 8:00", "2015-12-15 9:00")
				.WithMappedRule("phone", phone, 0, Adherence.In);
			Now.Is("2015-12-15 8:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Database.PersistedReadModel.IsRuleAlarm.Should().Be(false);
		}
	}
}
