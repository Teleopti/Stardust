using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service.AgentStateReadModel.StartTime
{
	[RtaTest]
	[TestFixture]
	public class OutOfAdherenceStartTimeTest
	{
		public FakeDatabase Database;
		public MutableNow Now;
		public Rta Target;
		public FakeAgentStateReadModelPersister ReadModels;

		[Test]
		public void ShouldHaveOutOfAdherenceStartTime()
		{
			var personId = Guid.NewGuid();
			var admin = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, admin, "2017-11-07 8:00", "2017-11-07 9:00")
				.WithMappedRule("phone", admin, 0, Adherence.Configuration.Adherence.Out)
				.WithAlarm(TimeSpan.FromMinutes(5));

			Now.Is("2017-11-07 8:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			ReadModels.Models.Single(x => x.PersonId == personId)
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
				.WithMappedRule("loggedoff", phone, 0, Adherence.Configuration.Adherence.Out)
				.WithMappedRule("phone", phone, 0, Adherence.Configuration.Adherence.In)
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

			ReadModels.Models.Single(x => x.PersonId == personId)
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
				.WithMappedRule("ready", admin, 0, Adherence.Configuration.Adherence.Out)
				.WithMappedRule("admin", admin, 0, Adherence.Configuration.Adherence.Neutral)
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

			ReadModels.Models.Single(x => x.PersonId == personId)
				.OutOfAdherenceStartTime.Should().Be(null);
		}
	}
}