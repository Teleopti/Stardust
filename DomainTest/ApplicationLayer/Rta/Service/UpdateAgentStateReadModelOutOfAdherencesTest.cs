using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	[Toggle(Toggles.RTA_RecentOutOfAdherences_39145)]
	public class UpdateAgentStateReadModelOutOfAdherencesTest
	{
		public FakeRtaDatabase Database;
		public MutableNow Now;
		public IJsonDeserializer Deserializer;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldPersistOutOfAdherence()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", person)
				.WithSchedule(person, phone, "2016-05-30 09:00", "2016-05-30 10:00")
				.WithRule("out", phone, -1, Adherence.Out);

			Now.Is("2016-05-30 09:00");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "out"
			});

			var outOfAdherence = Database.PersistedReadModel.OutOfAdherences.Single();
			outOfAdherence.StartTime.Should().Be("2016-05-30 09:00".Utc());
			outOfAdherence.EndTime.Should().Be(null);
		}

		[Test]
		public void ShouldPersistOutOfAdherences()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", person)
				.WithSchedule(person, phone, "2016-05-30 10:00", "2016-05-30 11:00")
				.WithRule("out", phone, -1, Adherence.Out)
				.WithRule("ready", phone, 0, Adherence.In);

			Now.Is("2016-05-30 10:00");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "out"
			});
			Now.Is("2016-05-30 10:05");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "ready"
			});
			Now.Is("2016-05-30 10:10");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "out"
			});

			var outOfAdherences = Database.PersistedReadModel.OutOfAdherences;
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
				.WithUser("usercode", person)
				.WithSchedule(person, phone, "2016-05-30 10:00", "2016-05-30 11:00")
				.WithRule("out", phone, -1, Adherence.Out)
				.WithRule("ready", phone, 0, Adherence.In);

			Now.Is("2016-05-30 09:55");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "out"
			});
			Now.Is("2016-05-30 10:00");
			Target.CheckForActivityChanges(Database.TenantName());

			var outOfAdherence = Database.PersistedReadModel.OutOfAdherences.Single();
			outOfAdherence.StartTime.Should().Be("2016-05-30 10:00".Utc());
			outOfAdherence.EndTime.Should().Be(null);
		}

		[Test]
		public void ShouldConsiderFromPastActivityChange()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", person)
				.WithSchedule(person, phone, "2016-05-30 10:00", "2016-05-30 11:00")
				.WithRule("out", phone, -1, Adherence.Out)
				.WithRule("ready", phone, 0, Adherence.In);

			Now.Is("2016-05-30 09:55");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "out"
			});
			Now.Is("2016-05-30 10:05");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "ready"
			});

			var outOfAdherence = Database.PersistedReadModel.OutOfAdherences.Single();
			outOfAdherence.StartTime.Should().Be("2016-05-30 10:00".Utc());
			outOfAdherence.EndTime.Should().Be("2016-05-30 10:05".Utc());
		}

		[Test]
		public void ShouldConsiderNeutralAdherence()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", person)
				.WithSchedule(person, phone, "2016-05-30 09:00", "2016-05-30 10:00")
				.WithRule("out", phone, -1, Adherence.Out)
				.WithRule("admin", phone, 0, Adherence.Neutral)
				;

			Now.Is("2016-05-30 09:00");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "out"
			});
			Now.Is("2016-05-30 09:05");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});

			var outOfAdherence = Database.PersistedReadModel.OutOfAdherences.Single();
			outOfAdherence.StartTime.Should().Be("2016-05-30 09:00".Utc());
			outOfAdherence.EndTime.Should().Be("2016-05-30 09:05".Utc());
		}

		[Test]
		public void ShouldConsiderNeutralToInAdherence()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", person)
				.WithSchedule(person, phone, "2016-05-30 09:00", "2016-05-30 10:00")
				.WithRule("out", phone, -1, Adherence.Out)
				.WithRule("admin", phone, 0, Adherence.Neutral)
				.WithRule("ready", phone, 0, Adherence.In)
				;

			Now.Is("2016-05-30 09:00");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "out"
			});
			Now.Is("2016-05-30 09:05");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});
			Now.Is("2016-05-30 09:10");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "ready"
			});

			var outOfAdherence = Database.PersistedReadModel.OutOfAdherences.Single();
			outOfAdherence.StartTime.Should().Be("2016-05-30 09:00".Utc());
			outOfAdherence.EndTime.Should().Be("2016-05-30 09:05".Utc());
		}

		[Test]
		public void ShouldConsiderInToNeutralAdherence()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", person)
				.WithSchedule(person, phone, "2016-05-30 09:00", "2016-05-30 10:00")
				.WithRule("out", phone, -1, Adherence.Out)
				.WithRule("admin", phone, 0, Adherence.Neutral)
				.WithRule("ready", phone, 0, Adherence.In)
				;

			Now.Is("2016-05-30 09:00");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "out"
			});
			Now.Is("2016-05-30 09:05");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "ready"
			});
			Now.Is("2016-05-30 09:10");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});

			var outOfAdherence = Database.PersistedReadModel.OutOfAdherences.Single();
			outOfAdherence.StartTime.Should().Be("2016-05-30 09:00".Utc());
			outOfAdherence.EndTime.Should().Be("2016-05-30 09:05".Utc());
		}

		[Test]
		public void ShouldExcludeOutOfAdherencesEndingOverAnHourAgo()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", person)
				.WithSchedule(person, phone, "2016-05-30 10:00", "2016-05-30 17:00")
				.WithRule("out", phone, -1, Adherence.Out)
				.WithRule("ready", phone, 0, Adherence.In);

			Now.Is("2016-05-30 10:00");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "out"
			});
			Now.Is("2016-05-30 10:05");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "ready"
			});
			Now.Is("2016-05-30 11:15");
			Target.CheckForActivityChanges(Database.TenantName());

			Database.PersistedReadModel.OutOfAdherences
				.Should().Be.Empty();
		}
	}
}