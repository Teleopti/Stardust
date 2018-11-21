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
using Teleopti.Wfm.Adherence.Domain.Service;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service
{
	
	[TestFixture]
	[RtaTest]
	public class PersonOutOfAdherenceEventTest
	{
		public FakeDatabase Database;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public Rta Target;

		[Test]
		public void ShouldPublishOnPositiveStaffingEffect()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithMappedRule("statecode", activityId, 1)
				;
			Now.Is("2014-10-20 9:00");

			Target.ProcessState(new StateForTest
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
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithMappedRule("statecode", activityId, -1)
				;
			Now.Is("2014-10-20 9:00");

			Target.ProcessState(new StateForTest
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
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithMappedRule("statecode1", activityId, -1)
				.WithMappedRule("statecode2", activityId, 1)
				;
			Now.Is("2014-10-20 9:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode1"
			});
			Target.ProcessState(new StateForTest
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
				.WithAgent("usercode", person)
				.WithSchedule(person, phone, "2014-10-20 9:00", "2014-10-20 10:00")
				.WithMappedRule(null, phone, 0)
				.WithMappedRule("loggedoff", phone, -1);
			Now.Is("2014-10-20 9:05");

			Target.ProcessState(new StateForTest
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
				.WithAgent("usercode", person)
				.WithSchedule(person, phone, "2014-10-20 9:00", "2014-10-20 10:00")
				.WithMappedRule(null, phone, -1)
				.WithMappedRule("loggedoff", phone, -1);
			Now.Is("2014-10-20 0:00");
			Target.CheckForActivityChanges(Database.TenantName());
			Publisher.Clear();
			Now.Is("2014-10-20 9:05");

			Target.ProcessState(new StateForTest
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
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "2014-11-11 10:00", "2014-11-11 12:00")
				.WithMappedRule("statecode", activityId, -1);
			Now.Is("2014-11-11 11:00");

			Target.ProcessState(new StateForTest
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
				.WithAgent("usercode", personId, null, teamId, null)
				.WithSchedule(personId, activityId, "2014-10-20 9:00", "2014-10-20 10:00")
				.WithMappedRule("statecode", activityId, -1);
			Now.Is("2014-10-20 9:00");

			Target.ProcessState(new StateForTest
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
				.WithAgent("usercode", person)
				.WithSchedule(person, phone, "2015-11-25 8:00", "2015-11-25 12:00")
				.WithMappedRule("phone", phone, 0)
				.WithMappedRule("phone", null, +1)
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

			Publisher.PublishedEvents.OfType<PersonOutOfAdherenceEvent>()
				.Single().Timestamp.Should().Be("2015-11-25 12:00".Utc());
		}

		[Test]
		public void ShouldPublishWhenOutOfAfterShiftEnds()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", person)
				.WithSchedule(person, phone, "2015-11-25 8:00", "2015-11-25 12:00")
				.WithMappedRule("phone", phone, 0)
				.WithMappedRule("phone", null, 1)
				.WithMappedRule("logged off", phone, -1)
				.WithMappedRule("logged off", null, 0)
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

			Publisher.PublishedEvents.OfType<PersonOutOfAdherenceEvent>()
				.Single().Timestamp.Should().Be("2015-11-25 12:01".Utc());
		}

		[Test]
		public void ShouldNotPublishDuplicateEventsBecauseOfRuleChanges()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", person)
				.WithSchedule(person, phone, "2016-05-30 09:00", "2016-05-30 10:00")
				.WithMappedRule("state1", phone, -1, Domain.Configuration.Adherence.Out)
				.WithMappedRule("state2", phone, -1, Domain.Configuration.Adherence.Out)
				;

			Now.Is("2016-05-30 09:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "state1"
			});
			Database.ClearRuleMap()
				.WithMappedRule("state1", phone, 0, Domain.Configuration.Adherence.In)
				.WithMappedRule("state2", phone, -1, Domain.Configuration.Adherence.Out)
				;
			Now.Is("2016-05-30 09:01");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "state2"
			});

			Publisher.PublishedEvents.OfType<PersonOutOfAdherenceEvent>()
				.Single().Timestamp.Should().Be("2016-05-30 09:00".Utc());
		}
	}
}