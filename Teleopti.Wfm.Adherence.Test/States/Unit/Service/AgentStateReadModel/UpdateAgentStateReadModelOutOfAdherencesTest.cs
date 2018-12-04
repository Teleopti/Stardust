using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.Domain.Service;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service.AgentStateReadModel
{
	[TestFixture]
	[RtaTest]
	public class UpdateAgentStateReadModelOutOfAdherencesTest
	{
		public FakeDatabase Database;
		public MutableNow Now;
		public Rta Target;
		public FakeAgentStateReadModelPersister ReadModels;

		[Test]
		public void ShouldPersistOutOfAdherence()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", person)
				.WithSchedule(person, phone, "2016-05-30 09:00", "2016-05-30 10:00")
				.WithMappedRule("out", phone, -1, Adherence.Configuration.Adherence.Out);

			Now.Is("2016-05-30 09:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "out"
			});

			var outOfAdherence = ReadModels.Models.Single(x => x.PersonId == person)
				.OutOfAdherences.Single();
			outOfAdherence.StartTime.Should().Be("2016-05-30 09:00".Utc());
			outOfAdherence.EndTime.Should().Be(null);
		}

		[Test]
		public void ShouldPersistOutOfAdherences()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", person)
				.WithSchedule(person, phone, "2016-05-30 10:00", "2016-05-30 11:00")
				.WithMappedRule("out", phone, -1, Adherence.Configuration.Adherence.Out)
				.WithMappedRule("ready", phone, 0, Adherence.Configuration.Adherence.In);

			Now.Is("2016-05-30 10:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "out"
			});
			Now.Is("2016-05-30 10:05");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "ready"
			});
			Now.Is("2016-05-30 10:10");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "out"
			});

			var outOfAdherences = ReadModels.Models.Single(x => x.PersonId == person)
				.OutOfAdherences;
			outOfAdherences.First().StartTime.Should().Be("2016-05-30 10:00".Utc());
			outOfAdherences.First().EndTime.Should().Be("2016-05-30 10:05".Utc());
			outOfAdherences.Last().StartTime.Should().Be("2016-05-30 10:10".Utc());
			outOfAdherences.Last().EndTime.Should().Be(null);
		}

		[Test]
		public void ShouldConsiderActivityChange()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", person)
				.WithSchedule(person, phone, "2016-05-30 10:00", "2016-05-30 11:00")
				.WithMappedRule("out", phone, -1, Adherence.Configuration.Adherence.Out)
				.WithMappedRule("ready", phone, 0, Adherence.Configuration.Adherence.In);

			Now.Is("2016-05-30 09:55");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "out"
			});
			Now.Is("2016-05-30 10:00");
			Target.CheckForActivityChanges(Database.TenantName());

			var outOfAdherence = ReadModels.Models.Single(x => x.PersonId == person)
				.OutOfAdherences.Single();
			outOfAdherence.StartTime.Should().Be("2016-05-30 10:00".Utc());
			outOfAdherence.EndTime.Should().Be(null);
		}

		[Test]
		public void ShouldConsiderFromPastActivityChange()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", person)
				.WithSchedule(person, phone, "2016-05-30 10:00", "2016-05-30 11:00")
				.WithMappedRule("out", phone, -1, Adherence.Configuration.Adherence.Out)
				.WithMappedRule("ready", phone, 0, Adherence.Configuration.Adherence.In);

			Now.Is("2016-05-30 09:55");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "out"
			});
			Now.Is("2016-05-30 10:05");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "ready"
			});

			var outOfAdherence = ReadModels.Models.Single(x => x.PersonId == person)
				.OutOfAdherences.Single();
			outOfAdherence.StartTime.Should().Be("2016-05-30 10:00".Utc());
			outOfAdherence.EndTime.Should().Be("2016-05-30 10:05".Utc());
		}

		[Test]
		public void ShouldConsiderNeutralAdherence()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", person)
				.WithSchedule(person, phone, "2016-05-30 09:00", "2016-05-30 10:00")
				.WithMappedRule("out", phone, -1, Adherence.Configuration.Adherence.Out)
				.WithMappedRule("admin", phone, 0, Adherence.Configuration.Adherence.Neutral)
				;

			Now.Is("2016-05-30 09:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "out"
			});
			Now.Is("2016-05-30 09:05");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});

			var outOfAdherence = ReadModels.Models.Single(x => x.PersonId == person)
				.OutOfAdherences.Single();
			outOfAdherence.StartTime.Should().Be("2016-05-30 09:00".Utc());
			outOfAdherence.EndTime.Should().Be("2016-05-30 09:05".Utc());
		}

		[Test]
		public void ShouldConsiderNeutralToInAdherence()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", person)
				.WithSchedule(person, phone, "2016-05-30 09:00", "2016-05-30 10:00")
				.WithMappedRule("out", phone, -1, Adherence.Configuration.Adherence.Out)
				.WithMappedRule("admin", phone, 0, Adherence.Configuration.Adherence.Neutral)
				.WithMappedRule("ready", phone, 0, Adherence.Configuration.Adherence.In)
				;

			Now.Is("2016-05-30 09:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "out"
			});
			Now.Is("2016-05-30 09:05");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});
			Now.Is("2016-05-30 09:10");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "ready"
			});

			var outOfAdherence = ReadModels.Models.Single(x => x.PersonId == person)
				.OutOfAdherences.Single();
			outOfAdherence.StartTime.Should().Be("2016-05-30 09:00".Utc());
			outOfAdherence.EndTime.Should().Be("2016-05-30 09:05".Utc());
		}

		[Test]
		public void ShouldConsiderInToNeutralAdherence()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", person)
				.WithSchedule(person, phone, "2016-05-30 09:00", "2016-05-30 10:00")
				.WithMappedRule("out", phone, -1, Adherence.Configuration.Adherence.Out)
				.WithMappedRule("admin", phone, 0, Adherence.Configuration.Adherence.Neutral)
				.WithMappedRule("ready", phone, 0, Adherence.Configuration.Adherence.In)
				;

			Now.Is("2016-05-30 09:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "out"
			});
			Now.Is("2016-05-30 09:05");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "ready"
			});
			Now.Is("2016-05-30 09:10");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});

			var outOfAdherence = ReadModels.Models.Single(x => x.PersonId == person)
				.OutOfAdherences.Single();
			outOfAdherence.StartTime.Should().Be("2016-05-30 09:00".Utc());
			outOfAdherence.EndTime.Should().Be("2016-05-30 09:05".Utc());
		}

		[Test]
		public void ShouldExcludeOutOfAdherencesEndingOverAnHourAgo()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", person)
				.WithSchedule(person, phone, "2016-05-30 10:00", "2016-05-30 17:00")
				.WithMappedRule("out", phone, -1, Adherence.Configuration.Adherence.Out)
				.WithMappedRule("ready", phone, 0, Adherence.Configuration.Adherence.In)
				.WithMappedRule("incall", phone, 0, Adherence.Configuration.Adherence.In);

			Now.Is("2016-05-30 10:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "out"
			});
			Now.Is("2016-05-30 10:05");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "ready"
			});
			Now.Is("2016-05-30 11:15");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "incall"
			});

			ReadModels.Models.Single(x => x.PersonId == person)
				.OutOfAdherences
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotConsiderOutToOutBecauseOfRuleChanges()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", person)
				.WithSchedule(person, phone, "2016-05-30 09:00", "2016-05-30 10:00")
				.WithMappedRule("state1", phone, -1, Adherence.Configuration.Adherence.Out)
				.WithMappedRule("state2", phone, -1, Adherence.Configuration.Adherence.Out)
				;

			Now.Is("2016-05-30 09:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "state1"
			});
			Database.ClearRuleMap()
				.WithMappedRule("state1", phone, 0, Adherence.Configuration.Adherence.In)
				.WithMappedRule("state2", phone, -1, Adherence.Configuration.Adherence.Out)
				;
			Now.Is("2016-05-30 09:01");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "state2"
			});

			var outOfAdherence = ReadModels.Models.Single(x => x.PersonId == person)
				.OutOfAdherences.Single();
			outOfAdherence.StartTime.Should().Be("2016-05-30 09:00".Utc());
			outOfAdherence.EndTime.Should().Be(null);
		}
	}
}