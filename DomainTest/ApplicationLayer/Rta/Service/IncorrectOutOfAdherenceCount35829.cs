using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	public class IncorrectOutOfAdherenceCount35829
	{
		public FakeRtaDatabase Database;
		public MutableNow Now;
		public FakeEventPublisher Publisher;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldWork_RealScenario()
		{
			var person = Guid.NewGuid();
			var inbound = Guid.NewGuid();
			var breaks = Guid.NewGuid();
			var lunch = Guid.NewGuid();

			Database
				.WithAgent("usercode", person)
				.WithSchedule(person, inbound, "2015-11-25 14:00:00", "2015-11-25 15:05:00")
				.WithSchedule(person, breaks, "2015-11-25 15:05:00", "2015-11-25 15:15:00")
				.WithSchedule(person, inbound, "2015-11-25 15:15:00", "2015-11-25 17:15:00")
				.WithSchedule(person, lunch, "2015-11-25 17:15:00", "2015-11-25 17:45:00")
				.WithSchedule(person, inbound, "2015-11-25 17:45:00", "2015-11-25 20:20:00")
				.WithSchedule(person, breaks, "2015-11-25 20:20:00", "2015-11-25 20:30:00")
				.WithSchedule(person, inbound, "2015-11-25 20:30:00", "2015-11-25 23:00:00")
				.WithRule("1", inbound, -1, Adherence.Out)
				.WithRule("1", breaks, 0, Adherence.In)
				.WithRule("1", lunch, 0, Adherence.In)
				.WithRule("1", null, 0, Adherence.In)
				;

			Now.Is("2015-11-25 13:55");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "1"
			});

			Now.Is("2015-11-25 14:00");
			Target.CheckForActivityChanges(Database.TenantName());

			Now.Is("2015-11-25 15:05");
			Target.CheckForActivityChanges(Database.TenantName());

			Now.Is("2015-11-25 15:15");
			Target.CheckForActivityChanges(Database.TenantName());

			Now.Is("2015-11-25 17:15");
			Target.CheckForActivityChanges(Database.TenantName());

			Now.Is("2015-11-25 17:45");
			Target.CheckForActivityChanges(Database.TenantName());

			Now.Is("2015-11-25 20:20");
			Target.CheckForActivityChanges(Database.TenantName());

			Now.Is("2015-11-25 20:30");
			Target.CheckForActivityChanges(Database.TenantName());
			Publisher.Clear();
			
			Now.Is("2015-11-25 23:01");
			Target.CheckForActivityChanges(Database.TenantName());

			Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>()
				.Single().Timestamp.Should().Be("2015-11-25 23:00".Utc());
		}

	}
}