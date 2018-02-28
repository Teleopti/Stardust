using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service.AgentStateReadModel.StartTime
{
	[RtaTest]
	[TestFixture]
	public class OutOfAdherenceStartTimeTest
	{
		public FakeDatabase Database;
		public MutableNow Now;
		public Domain.Rta.Service.Rta Target;

		[Test]
		public void ShouldHaveOutOfAdherenceStartTime()
		{
			var personId = Guid.NewGuid();
			var admin = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, admin, "2017-11-07 8:00", "2017-11-07 9:00")
				.WithMappedRule("phone", admin, 0, Adherence.Out)
				.WithAlarm(TimeSpan.FromMinutes(5));

			Now.Is("2017-11-07 8:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Database.PersistedReadModel
				.OutOfAdherenceStartTime.Should().Be("2017-11-07 8:00".Utc());
		}
		
		[Test]
		public void ShouldNotHaveOutOfAdherenceStartTimeWhenInAdherence()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2017-11-07 8:00", "2017-11-07 9:00")
				.WithMappedRule("loggedoff", phone, 0, Adherence.Out)
				.WithMappedRule("phone", phone, 0, Adherence.In)
				.WithAlarm(TimeSpan.FromMinutes(5));

			Now.Is("2017-11-07 8:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "loggedoff"
			});
			Now.Is("2017-11-07 8:01");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Database.PersistedReadModel
				.OutOfAdherenceStartTime.Should().Be(null);
		}
		
		[Test]
		public void ShouldNotHaveOutOfAdherenceStartTimeWhenNeutralAdherence()
		{
			var personId = Guid.NewGuid();
			var admin = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, admin, "2017-11-07 8:00", "2017-11-07 9:00")
				.WithMappedRule("ready", admin, 0, Adherence.Out)
				.WithMappedRule("admin", admin, 0, Adherence.Neutral)
				.WithAlarm(TimeSpan.FromMinutes(5));

			Now.Is("2017-11-07 8:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "ready"
			});
			Now.Is("2017-11-07 8:01");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});

			Database.PersistedReadModel
				.OutOfAdherenceStartTime.Should().Be(null);
		}
	}
}