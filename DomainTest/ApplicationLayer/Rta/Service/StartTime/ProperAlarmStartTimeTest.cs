using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service.StartTime
{
	[RtaTest]
	[TestFixture]
	[Toggle(Toggles.Wfm_RTA_ProperAlarm_34975)]
	public class ProperAlarmStartTimeTest
	{
		public FakeRtaDatabase Database;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldHaveAlarmStartTimeWhenEnteringAlarm()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2015-12-10 8:00", "2015-12-10 9:00")
				.WithRule("phone", phone, 0, Adherence.In, TimeSpan.FromMinutes(5));
			Now.Is("2015-12-10 8:00");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Database.PersistedReadModel.AlarmStartTime.Should().Be("2015-12-10 8:05".Utc());
		}


		[Test]
		public void ShouldNotHaveAlarmStartTimeWhenNotInAlarm()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2015-12-10 8:00", "2015-12-10 9:00")
				.WithRule("phone", phone, 0, Adherence.In);
			Now.Is("2015-12-10 8:00");

			Target.SaveState(new ExternalUserStateForTest
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
			var inAdherenceRule = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2015-12-10 8:00", "2015-12-10 9:00")
				.WithRule("phone", phone, inAdherenceRule, 0, Adherence.In, "5".Minutes())
				.WithRule("ACW", phone, inAdherenceRule, 0, Adherence.In, "5".Minutes())
				;
			Now.Is("2015-12-10 8:00");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Now.Is("2015-12-10 8:10");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "ACW"
			});

			Database.PersistedReadModel.AlarmStartTime.Should().Be("2015-12-10 8:05".Utc());
		}
	}
}
