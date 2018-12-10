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
	public class EventsStreamTest
	{
		public FakeDatabase Database;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public Rta Target;

		[Test]
		public void ShouldPublishAdherenceEventsFor2Changes()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var brejk = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2014-10-20 9:00", "2014-10-20 10:00")
				.WithSchedule(personId, brejk, "2014-10-20 10:00", "2014-10-20 10:15")
				.WithMappedRule("phone", phone, 0)
				.WithMappedRule("phone", brejk, 1)
				.WithMappedRule("break", brejk, 0)
				.WithMappedRule("break", phone, 1);
			Now.Is("2014-10-20 9:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Publisher.Clear();

			Now.Is("2014-10-20 10:02");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "break"
			});

			Publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single().StartTime.Should().Be("2014-10-20 10:00".Utc());
			Publisher.PublishedEvents.OfType<PersonOutOfAdherenceEvent>().Single().Timestamp.Should().Be("2014-10-20 10:00".Utc());
			Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single().Timestamp.Should().Be("2014-10-20 10:02".Utc());
			Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single().Adherence.Should().Be(EventAdherence.In);
			Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Single().Timestamp.Should().Be("2014-10-20 10:02".Utc());
		}

		[Test]
		public void ShouldPublishActivityAndAdherenceEventsFromNowWhenRewritingHistory()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var admin = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2014-10-20 9:00", "2014-10-20 10:00")
				.WithMappedRule("admin", phone, 1)
				.WithMappedRule("admin", admin, 0);
			Now.Is("2014-10-20 9:15");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});
			Publisher.Clear();

			Now.Is("2014-10-20 9:30");
			Database.ClearAssignments(personId);
			Database.WithSchedule(personId, admin, "2014-10-20 9:00", "2014-10-20 10:00");
			Target.CheckForActivityChanges(Database.TenantName(), personId);

			Publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single().StartTime.Should().Be("2014-10-20 9:30".Utc());
			Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Single().Timestamp.Should().Be("2014-10-20 9:30".Utc());
		}

		[Test]
		public void ShouldPublishRuleEventsFor2Changes()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var brejk = Guid.NewGuid();
			var inAdherence = Guid.NewGuid();
			var outOfAdherence = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2017-03-13 9:00", "2017-03-13 10:00")
				.WithSchedule(personId, brejk, "2017-03-13 10:00", "2017-03-13 10:15")
				.WithMappedRule("phone", phone, inAdherence, "in")
				.WithMappedRule("phone", brejk, outOfAdherence, "out")
				.WithMappedRule("break", brejk, inAdherence, "in")
				.WithMappedRule("break", phone, outOfAdherence, "out");
			Now.Is("2017-03-13 9:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Publisher.Clear();

			Now.Is("2017-03-13 10:02");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "break"
			});

			Publisher.PublishedEvents.OfType<PersonRuleChangedEvent>()
				.Single(x => x.RuleName == "out")
				.Timestamp.Should().Be("2017-03-13 10:00".Utc());
			Publisher.PublishedEvents.OfType<PersonRuleChangedEvent>()
				.Single(x => x.RuleName == "in")
				.Timestamp.Should().Be("2017-03-13 10:02".Utc());
		}

		[Test]
		public void ShouldPublishRuleEventsFor2ChangesAfterShift()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var inAdherence = Guid.NewGuid();
			var outOfAdherence = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2017-03-13 9:00", "2017-03-13 10:00")
				.WithMappedRule("phone", phone, inAdherence, "in")
				.WithMappedRule("phone", null, outOfAdherence, "out")
				.WithMappedRule("loggedoff", null, inAdherence, "in")
				.WithMappedRule("loggedoff", phone, outOfAdherence, "out");
			Now.Is("2017-03-13 9:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Publisher.Clear();

			Now.Is("2017-03-13 10:02");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "loggedoff"
			});

			Publisher.PublishedEvents.OfType<PersonRuleChangedEvent>()
				.Single(x => x.RuleName == "out")
				.Timestamp.Should().Be("2017-03-13 10:00".Utc());
			Publisher.PublishedEvents.OfType<PersonRuleChangedEvent>()
				.Single(x => x.RuleName == "in")
				.Timestamp.Should().Be("2017-03-13 10:02".Utc());
		}

		[Test]
		public void ShouldPublishRuleEventsFor2ChangesAfterShortActivity()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var brejk = Guid.NewGuid();
			var inAdherence = Guid.NewGuid();
			var outOfAdherence = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2017-03-13 9:00", "2017-03-13 10:00")
				.WithSchedule(personId, brejk, "2017-03-13 10:00", "2017-03-13 10:01")
				.WithSchedule(personId, phone, "2017-03-13 10:01", "2017-03-13 11:00")
				.WithMappedRule("phone", phone, inAdherence, "in")
				.WithMappedRule("phone", brejk, outOfAdherence, "out")
				.WithMappedRule("break", brejk, inAdherence, "in")
				.WithMappedRule("break", phone, outOfAdherence, "out");
			Now.Is("2017-03-13 9:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Publisher.Clear();

			Now.Is("2017-03-13 10:02");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Publisher.PublishedEvents.OfType<PersonRuleChangedEvent>()
				.Single(x => x.RuleName == "out")
				.Timestamp.Should().Be("2017-03-13 10:00".Utc());
			Publisher.PublishedEvents.OfType<PersonRuleChangedEvent>()
				.Single(x => x.RuleName == "in")
				.Timestamp.Should().Be("2017-03-13 10:01".Utc());
		}


	}
}