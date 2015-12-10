using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service.StartTime
{
	[RtaTest]
	[TestFixture]
	public class AdherenceStartTimeTest
	{
		public FakeRtaDatabase Database;
		public FakeMessageSender Sender;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldHaveAdherenceStartTimeWhenHavingAdherence()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, phone, "2015-12-10 8:00", "2015-12-10 9:00")
				.WithAlarm("phone", phone, 0, Adherence.In, TimeSpan.FromMinutes(5));

			Now.Is("2015-12-10 8:00");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Sender.NotificationsOfType<AgentStateReadModel>().Last().DeseralizeActualAgentState()
			.AdherenceStartTime.Should().Be("2015-12-10 8:00".Utc());
		}

		[Test]
		public void ShouldNotChangeAherenceStartTimeWhenAdherenceDoesNotChange()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, phone, "2015-12-10 8:00", "2015-12-10 9:00")
				.WithAlarm("phone", phone, 0, Adherence.In, TimeSpan.FromMinutes(5))
				.WithAlarm("ready", phone, 0, Adherence.In, TimeSpan.FromMinutes(5));

			Now.Is("2015-12-10 8:00");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Now.Is("2015-12-10 8:30");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "ready"
			});

			Sender.NotificationsOfType<AgentStateReadModel>().Last().DeseralizeActualAgentState()
			.AdherenceStartTime.Should().Be("2015-12-10 8:00".Utc());
		}

		[Test]
		public void ShouldUpdateAherenceStartTimeWhenAdherenceChanges()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, phone, "2015-12-10 8:00", "2015-12-10 9:00")
				.WithAlarm("phone", phone, 0, Adherence.In, TimeSpan.FromMinutes(5))
				.WithAlarm("break", phone, -1, Adherence.Out, TimeSpan.FromMinutes(5));

			Now.Is("2015-12-10 8:00");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Now.Is("2015-12-10 8:30");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "break"
			});

			Sender.NotificationsOfType<AgentStateReadModel>().Last().DeseralizeActualAgentState()
			.AdherenceStartTime.Should().Be("2015-12-10 8:30".Utc());
		}
	}
}
