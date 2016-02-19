using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service.StartTime
{
	[RtaTest]
	[Toggle(Toggles.Wfm_RTA_ProperAlarm_34975)]
	[TestFixture]
	public class IsAlarmTest
	{
		public FakeRtaDatabase Database;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[ToggleOff(Toggles.Wfm_RTA_ProperAlarm_34975)]
		[Test]
		public void ShouldBeInAlarmEvenRuleIsNotAnAlarm()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2015-12-15 8:00", "2015-12-15 9:00")
				.WithRule("phone", phone, 0, Adherence.In);
			Now.Is("2015-12-15 8:00");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Database.PersistedReadModel.IsAlarm.Should().Be(true);
		}
		
		[Test]
		public void ShouldBeInAlarmIfEnteredRuleIsAlarm()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2015-12-15 8:00", "2015-12-15 9:00")
				.WithRule("phone", phone, 0, Adherence.In, TimeSpan.FromMinutes(5));
			Now.Is("2015-12-15 8:00");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Database.PersistedReadModel.IsAlarm.Should().Be(true);
		}

		[Test]
		public void ShouldNotBeInAlarmIfEnteredRuleIsNotAlarm()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2015-12-15 8:00", "2015-12-15 9:00")
				.WithRule("phone", phone, 0, Adherence.In);
			Now.Is("2015-12-15 8:00");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Database.PersistedReadModel.IsAlarm.Should().Be(false);
		}
	}
}
