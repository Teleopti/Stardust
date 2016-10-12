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
	public class PersonOutOfAdherenceEventTest
	{
		public FakeRtaDatabase Database;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldPublishOnPositiveStaffingEffect()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithRule("statecode", activityId, 1)
				;
			Now.Is("2014-10-20 9:00");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonOutOfAdherenceEvent>().Single();
			@event.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldPublishOnNegativeStaffingEffect()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithRule("statecode", activityId, -1)
				;
			Now.Is("2014-10-20 9:00");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonOutOfAdherenceEvent>().Single();
			@event.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldNotPublishIfStillOutOfAdherence()
		{
			var activityId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithRule("statecode1", activityId, -1)
				.WithRule("statecode2", activityId, 1)
				;
			Now.Is("2014-10-20 9:00");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode1"
			});
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode2"
			});

			Publisher.PublishedEvents.OfType<PersonOutOfAdherenceEvent>().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPublishTimeOfStateChange()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", person)
				.WithSchedule(person, phone, "2014-10-20 9:00", "2014-10-20 10:00")
				.WithRule(null, phone, 0)
				.WithRule("loggedoff", phone, -1);
			Now.Is("2014-10-20 9:05");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "loggedoff"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonOutOfAdherenceEvent>().Single();
			@event.Timestamp.Should().Be("2014-10-20 9:05".Utc());
		}

		[Test]
		public void ShouldPublishTimeOfActivityChange()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", person)
				.WithSchedule(person, phone, "2014-10-20 9:00", "2014-10-20 10:00")
				.WithRule(null, phone, -1)
				.WithRule("loggedoff", phone, -1);
			Now.Is("2014-10-20 9:05");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "loggedoff"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonOutOfAdherenceEvent>().Single();
			@event.Timestamp.Should().Be("2014-10-20 9:00".Utc());
		}

		[Test]
		public void ShouldPublishWithBusinessUnitId()
		{
			var businessUnitId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithBusinessUnit(businessUnitId)
				.WithScenario(null, true)
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-11-11 10:00", "2014-11-11 12:00")
				.WithRule("statecode", activityId, -1);
			Now.Is("2014-11-11 11:00");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonOutOfAdherenceEvent>().Single();
			@event.BusinessUnitId.Should().Be(businessUnitId);
		}

		[Test]
		public void ShouldPublishWithTeamId()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId, null, teamId, null)
				.WithSchedule(personId, activityId, "2014-10-20 9:00", "2014-10-20 10:00")
				.WithRule("statecode", activityId, -1);
			Now.Is("2014-10-20 9:00");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonOutOfAdherenceEvent>().Single();
			@event.TeamId.Should().Be(teamId);
		}

		[Test]
		public void ShouldPublishWhenShiftEnds()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", person)
				.WithSchedule(person, phone, "2015-11-25 8:00", "2015-11-25 12:00")
				.WithRule("phone", phone, 0)
				.WithRule("phone", null, +1)
				;
			Now.Is("2015-11-25 8:00");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Publisher.Clear();

			Now.Is("2015-11-25 12:01");
			Target.CheckForActivityChanges(Database.TenantName());

			Publisher.PublishedEvents.OfType<PersonOutOfAdherenceEvent>()
				.Single().Timestamp.Should().Be("2015-11-25 12:00".Utc());
		}

		[Test]
		public void ShouldPublishWhenOutOfAfterShiftEnds()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", person)
				.WithSchedule(person, phone, "2015-11-25 8:00", "2015-11-25 12:00")
				.WithRule("phone", phone, 0)
				.WithRule("phone", null, 1)
				.WithRule("logged off", phone, -1)
				.WithRule("logged off", null, 0)
				;
			Now.Is("2015-11-25 11:55");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "logged off"
			});
			Publisher.Clear();

			Now.Is("2015-11-25 12:01");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Publisher.PublishedEvents.OfType<PersonOutOfAdherenceEvent>()
				.Single().Timestamp.Should().Be("2015-11-25 12:01".Utc());
		}

		[Test]
		public void ShouldNotPublishDuplicateEventsBecauseOfRuleChanges()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", person)
				.WithSchedule(person, phone, "2016-05-30 09:00", "2016-05-30 10:00")
				.WithRule("state1", phone, -1, Adherence.Out)
				.WithRule("state2", phone, -1, Adherence.Out)
				;

			Now.Is("2016-05-30 09:00");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "state1"
			});
			Database.ClearRuleMap()
				.WithRule("state1", phone, 0, Adherence.In)
				.WithRule("state2", phone, -1, Adherence.Out)
				;
			Now.Is("2016-05-30 09:01");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "state2"
			});

			Publisher.PublishedEvents.OfType<PersonOutOfAdherenceEvent>()
				.Single().Timestamp.Should().Be("2016-05-30 09:00".Utc());
		}
	}
}