using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	public class PersonRuleChangedEventTest
	{
		public FakeRtaDatabase Database;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldPublishEvent()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithMappedRule("statecode", null)
				;
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonRuleChangedEvent>().Single();
			@event.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldPublishOnlyIfChanged()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithMappedRule("state1", null)
				.WithStateGroup(null, "state2")
				.WithStateCode("state2")
				.WithMapping()
				.WithMappedRule("state3", null)
				;

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "state1"
			});
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "state2"
			});
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "state3"
			});

			var events = Publisher.PublishedEvents.OfType<PersonRuleChangedEvent>();
			events.Should().Have.Count.EqualTo(2);
		}

		[Test]
		public void ShouldPublishWithTimeOfStateChange()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithMappedRule("state1", null);
			Now.Is("2017-03-06 10:00");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "state1"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonRuleChangedEvent>().Single();
			@event.Timestamp.Should().Be("2017-03-06 10:00".Utc());
		}

		[Test]
		[Ignore("WIP")]
		public void ShouldPublishWithTimeOfActivityChange()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", person)
				.WithSchedule(person, phone, "2017-03-13 9:00", "2017-03-13 10:00")
				.WithMappedRule("state1", phone)
				.WithMappedRule(null, phone);
			Now.Is("2017-03-13 9:05");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "state1"
			});

			Publisher.PublishedEvents.OfType<PersonRuleChangedEvent>().First()
				.Timestamp.Should().Be("2017-03-13 9:00".Utc());
		}
		
		[Test]
		public void ShouldPublishWithRuleName()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2017-03-06 10:00", "2017-03-06 11:00")
				.WithMappedRule("break", phone, "out");
			Now.Is("2017-03-06 10:00");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "break"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonRuleChangedEvent>().Single();
			@event.RuleName.Should().Be("out");
		}

		[Test]
		public void ShouldPublishWithInAdherence()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.WithMappedRule("statecode", activityId, 0, Adherence.In);
			Now.Is("2014-10-20 10:00");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonRuleChangedEvent>().Single();
			@event.Adherence.Should().Be(EventAdherence.In);
		}

		[Test]
		public void ShouldPublishWithOutAdherence()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "2017-03-06 10:00", "2017-03-06 11:00")
				.WithMappedRule("statecode", activityId, 0, Adherence.Out);
			Now.Is("2017-03-06 10:00");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonRuleChangedEvent>().Single();
			@event.Adherence.Should().Be(EventAdherence.Out);
		}

		[Test]
		public void ShouldPublishWithNeutralAdherence()
		{
			var personId = Guid.NewGuid();
			var admin = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, admin, "2015-03-13 08:00", "2015-03-13 09:00")
				.WithMappedRule("admin", admin, 0, Adherence.Neutral);
			Now.Is("2015-03-13 08:00");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});

			Publisher.PublishedEvents.OfType<PersonRuleChangedEvent>().Single()
				.Adherence.Should().Be(EventAdherence.Neutral);
		}

		[Test]
		public void ShouldPublishWithActivity()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "phone", "2017-03-06 10:00", "2017-03-06 11:00")
				.WithMappedRule("admin", phone, 0, Adherence.Neutral);
			Now.Is("2017-03-06 10:00");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "break"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonRuleChangedEvent>().Single();
			@event.ActivityName.Should().Be("phone");
		}

		[Test]
		public void ShouldPublishWithActivityColor()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, Color.Green, "2017-03-06 10:00", "2017-03-06 11:00")
				.WithMappedRule("admin", phone, 0, Adherence.Neutral);
			Now.Is("2017-03-06 10:00");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "break"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonRuleChangedEvent>().Single();
			@event.ActivityColor.Should().Be(Color.Green.ToArgb());
		}

		[Test]
		public void ShouldPublishWithRuleColor()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2017-03-06 10:00", "2017-03-06 11:00")
				.WithMappedRule(Guid.NewGuid(), "break", phone, 0, "out", Adherence.Out, Color.DarkGoldenrod)
				;
			Now.Is("2017-03-06 10:00");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "break"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonRuleChangedEvent>().Single();
			@event.RuleColor.Should().Be(Color.DarkGoldenrod.ToArgb());
		}

		[Test]
		public void ShouldPublishWithStateName()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2017-03-06 10:00", "2017-03-06 11:00")
				.WithMappedRule(Guid.NewGuid(), "break", phone, 0, "out", Adherence.Out)
				;
			Now.Is("2017-03-06 10:00");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "break"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonRuleChangedEvent>().Single();
			@event.StateName.Should().Be("out");
		}

		[Test]
		public void ShouldPublishWithStateGroupId()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var stateGroup = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2017-03-06 10:00", "2017-03-06 11:00")
				.WithStateGroup(stateGroup, "out")
				.WithStateCode("break")
				.WithMappedRule(Guid.NewGuid(), "break", phone, 0, "out", Adherence.Out)
				;
			Now.Is("2017-03-06 10:00");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "break"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonRuleChangedEvent>().Single();
			@event.StateGroupId.Should().Be(stateGroup);
		}
	}

}