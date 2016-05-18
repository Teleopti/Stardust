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
	public class PersonActivityStartEventTest
	{
		public FakeRtaDatabase Database;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldPublishEvent()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				;
			Now.Is("2014-10-20 10:00");
			
			Target.CheckForActivityChanges(Database.TenantName(), personId);

			var @event = Publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single();
			@event.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldPublishIfNextActivityHasStarted()
		{
			var personId = Guid.NewGuid();
			var activityId1 = Guid.NewGuid();
			var activityId2 = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId1, "2014-10-20 10:00", "2014-10-20 10:15")
				.WithSchedule(personId, activityId2, "2014-10-20 10:15", "2014-10-20 11:00")
				;
			Now.Is("2014-10-20 10:00");

			Target.CheckForActivityChanges(Database.TenantName(), personId);
			Now.Is("2014-10-20 10:05");
			Target.CheckForActivityChanges(Database.TenantName(), personId);
			Now.Is("2014-10-20 10:15");
			Target.CheckForActivityChanges(Database.TenantName(), personId);

			var events = Publisher.PublishedEvents.OfType<PersonActivityStartEvent>();
			events.Should().Have.Count.EqualTo(2);
		}

		[Test]
		public void ShouldPublishWithActivityInfo()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "phone", "2014-10-20 10:00", "2014-10-20 11:00")
				;
			Now.Is("2014-10-20 10:02");

			Target.CheckForActivityChanges(Database.TenantName(), personId);

			var @event = Publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single();
			@event.StartTime.Should().Be("2014-10-20 10:00".Utc());
			@event.Name.Should().Be("phone");
		}
		
		[Test]
		public void ShouldPublishWithInAdherence()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2014-10-20 10:00", "2014-10-20 11:00")
				.WithRule("phone", phone, 0)
				;
			Now.Is("2014-10-20 09:50");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Now.Is("2014-10-20 10:02");
			Target.CheckForActivityChanges(Database.TenantName(), personId);

			var @event = Publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single();
			@event.Adherence.Should().Be(EventAdherence.In);
		}

		[Test]
		public void ShouldPublishWithOutAdherence()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "phone", "2014-10-20 10:00", "2014-10-20 11:00")
				.WithRule("statecode", activityId, 1)
				;
			Now.Is("2014-10-20 09:50");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});
			Now.Is("2014-10-20 10:02");
			Target.CheckForActivityChanges(Database.TenantName(), personId);

			var @event = Publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single();
			@event.Adherence.Should().Be(EventAdherence.Out);
		}
		
		[Test]
		public void ShouldPublishWithAdherenceBasedOnPreviousStateAndCurrentActivity()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "phone", "2014-10-20 10:00", "2014-10-20 11:00")
				.WithRule("admin", phone, 1, Adherence.Neutral)
				.WithRule("phone", phone, 0, Adherence.In);
			Now.Is("2014-10-20 09:50");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});
			Now.Is("2014-10-20 10:01");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single();
			@event.Adherence.Should().Be(EventAdherence.Neutral);
		}
	}
}