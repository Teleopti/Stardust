using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.Domain.Events;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service
{
	[TestFixture]
	[RtaTest]
	public class PersonNeutralAdherenceEventTest
	{
		public FakeDatabase Database;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public Rta Target;
		
		[Test]
		public void ShouldPublish()
		{
			var personId = Guid.NewGuid();
			var admin = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, admin, "2015-03-10 8:00", "2015-03-10 10:00")
				.WithMappedRule("admin", admin, 0, Adherence.Configuration.Adherence.Neutral);
			Now.Is("2015-03-10 8:30");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});

			Publisher.PublishedEvents.OfType<PersonNeutralAdherenceEvent>().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldNotPublishWhenInAdherence()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2015-03-10 8:00", "2015-03-10 10:00")
				.WithMappedRule("phone", phone, 0, Adherence.Configuration.Adherence.In);
			Now.Is("2015-03-10 8:00");
			Target.CheckForActivityChanges(Database.TenantName());
			Publisher.Clear();
			Now.Is("2015-03-10 8:30");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Publisher.PublishedEvents.OfType<PersonNeutralAdherenceEvent>().Should().Be.Empty();
		}

		[Test]
		public void ShouldNotPublishWhenOutOfAdherence()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2015-03-10 8:00", "2015-03-10 10:00")
				.WithMappedRule("break", phone, 0, Adherence.Configuration.Adherence.Out);
			Now.Is("2015-03-10 8:00");
			Target.CheckForActivityChanges(Database.TenantName());
			Publisher.Clear();
			Now.Is("2015-03-10 8:30");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "break"
			});

			Publisher.PublishedEvents.OfType<PersonNeutralAdherenceEvent>().Should().Be.Empty();
		}
		
		[Test]
		public void ShouldNotPublishIfStillNeutralAdherence()
		{
			var personId = Guid.NewGuid();
			var admin1 = Guid.NewGuid();
			var admin2 = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, admin1, "2015-03-10 8:00", "2015-03-10 9:00")
				.WithSchedule(personId, admin2, "2015-03-10 9:00", "2015-03-10 11:00")
				.WithMappedRule("admin1", admin1, 0, Adherence.Configuration.Adherence.Neutral)
				.WithMappedRule("admin1", admin2, 0, Adherence.Configuration.Adherence.Neutral)
				.WithMappedRule("admin2", admin1, 0, Adherence.Configuration.Adherence.Neutral)
				.WithMappedRule("admin2", admin2, 0, Adherence.Configuration.Adherence.Neutral)
				;

			Now.Is("2015-03-10 8:05");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "admin1"
			});
			Now.Is("2015-03-10 9:05");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "admin2"
			});

			Publisher.PublishedEvents.OfType<PersonNeutralAdherenceEvent>().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPublishWhenAlarmMappingIsMissing()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2015-03-19 8:00", "2015-03-19 9:00")
				.WithMappedRule()
				.WithMappedRule("phone", phone, 0, Adherence.Configuration.Adherence.In)
				;
			Now.Is("2015-03-19 8:01");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "someOtherCode"
			});

			Publisher.PublishedEvents.OfType<PersonNeutralAdherenceEvent>()
				.Where(x => x.Timestamp == "2015-03-19 8:01".Utc()).Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPublishForUnrecognizedState()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2015-03-19 8:00", "2015-03-19 9:00")
				.WithMappedRule("admin", phone, 0, Adherence.Configuration.Adherence.Neutral)
				.WithMappedRule("phone", phone, 0, Adherence.Configuration.Adherence.In)
				;
			Now.Is("2015-03-19 8:01");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "someOtherCode"
			});

			Publisher.PublishedEvents.OfType<PersonNeutralAdherenceEvent>()
				.Where(x => x.Timestamp == "2015-03-19 8:01".Utc()).Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPublishTimeOfStateChange()
		{
			var person = Guid.NewGuid();
			var admin = Guid.NewGuid();
			Database
				.WithAgent("usercode", person)
				.WithSchedule(person, admin, "2014-10-20 9:00", "2014-10-20 10:00")
				.WithMappedRule(null, admin, 0, Adherence.Configuration.Adherence.Out)
				.WithMappedRule("admin", admin, 0, Adherence.Configuration.Adherence.Neutral);
			Now.Is("2014-10-20 9:05");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonNeutralAdherenceEvent>().Single();
			@event.Timestamp.Should().Be("2014-10-20 9:05".Utc());
		}

		[Test]
		public void ShouldPublishTimeOfActivityChange()
		{
			var person = Guid.NewGuid();
			var admin = Guid.NewGuid();
			Database
				.WithAgent("usercode", person)
				.WithSchedule(person, admin, "2014-10-20 9:00", "2014-10-20 10:00")
				.WithMappedRule(null, null, 0, Adherence.Configuration.Adherence.In)
				.WithMappedRule(null, admin, 0, Adherence.Configuration.Adherence.Neutral)
				.WithMappedRule("admin", admin, 0, Adherence.Configuration.Adherence.Neutral);
			Now.Is("2014-10-20 0:00");
			Target.CheckForActivityChanges(Database.TenantName());
			Publisher.Clear();
			Now.Is("2014-10-20 9:05");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonNeutralAdherenceEvent>().Single();
			@event.Timestamp.Should().Be("2014-10-20 9:00".Utc());
		}

		[Test]
		public void ShouldPublishWithPersonTeamSiteIds()
		{
			var personId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var admin = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId, null, teamId, siteId)
				.WithSchedule(personId, admin, "2015-03-10 8:00", "2015-03-10 10:00")
				.WithMappedRule("admin", admin, 0, Adherence.Configuration.Adherence.Neutral)
				;
			Now.Is("2015-03-10 8:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonNeutralAdherenceEvent>().Single();
			@event.PersonId.Should().Be(personId);
			@event.TeamId.Should().Be(teamId);
			@event.SiteId.Should().Be(siteId);
		}

		[Test]
		public void ShouldPublishWithBusinessUnitId()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var admin = Guid.NewGuid();
			Database
				.WithBusinessUnit(businessUnitId)
				.WithAgent("usercode", personId, businessUnitId, null, null)
				.WithSchedule(personId, admin, "2015-03-10 8:00", "2015-03-10 10:00")
				.WithMappedRule("admin", admin, 0, Adherence.Configuration.Adherence.Neutral);
			Now.Is("2015-03-10 8:30");
			
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonNeutralAdherenceEvent>().Single();
			@event.BusinessUnitId.Should().Be(businessUnitId);
		}

		[Test]
		public void ShouldPublishWhenShiftEnds()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", person)
				.WithSchedule(person, phone, "2015-11-25 8:00", "2015-11-25 12:00")
				.WithMappedRule("phone", phone, 0, Adherence.Configuration.Adherence.In)
				.WithMappedRule("phone", null, +1, Adherence.Configuration.Adherence.Neutral)
				;
			Now.Is("2015-11-25 8:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Publisher.Clear();

			Now.Is("2015-11-25 12:01");
			Target.CheckForActivityChanges(Database.TenantName());

			Publisher.PublishedEvents.OfType<PersonNeutralAdherenceEvent>()
				.Single().Timestamp.Should().Be("2015-11-25 12:00".Utc());
		}

		[Test]
		public void ShouldPublishPersonWhenNeutralAfterShiftEnds()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", person)
				.WithSchedule(person, phone, "2015-11-25 8:00", "2015-11-25 12:00")
				.WithMappedRule("phone", phone, 0, Adherence.Configuration.Adherence.In)
				.WithMappedRule("phone", null, 1, Adherence.Configuration.Adherence.Neutral)
				.WithMappedRule("logged off", phone, -1, Adherence.Configuration.Adherence.Out)
				.WithMappedRule("logged off", null, 0, Adherence.Configuration.Adherence.In)
				;
			Now.Is("2015-11-25 11:55");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "logged off"
			});
			Publisher.Clear();

			Now.Is("2015-11-25 12:01");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Publisher.PublishedEvents.OfType<PersonNeutralAdherenceEvent>()
				.Single().Timestamp.Should().Be("2015-11-25 12:01".Utc());
		}

	}
}