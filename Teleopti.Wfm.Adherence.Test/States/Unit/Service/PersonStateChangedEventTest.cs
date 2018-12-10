using System;
using System.Drawing;
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
	public class PersonStateChangedEventTest
	{
		public FakeDatabase Database;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public Rta Target;

		[Test]
		public void ShouldPublishEvent()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithStateGroup(null, "default", true);

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single();
			@event.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldPublishEventOnlyIfStateChanged()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithMappedRule("stateone", Guid.NewGuid())
				.WithMappedRule("statetwo", Guid.NewGuid());

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "stateone"
			});
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "stateone"
			});
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statetwo"
			});

			var events = Publisher.PublishedEvents.OfType<PersonStateChangedEvent>();
			events.Should().Have.Count.EqualTo(2);
		}

		[Test]
		public void ShouldPublishWithSystemTime()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithStateCode("statecode");
			Now.Is("2014-10-20 10:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single();
			@event.Timestamp.Should().Be("2014-10-20 10:00".Utc());
		}
		
		[Test]
		public void ShouldPublishWithInAdherence()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.WithMappedRule("statecode", activityId, 0);
			Now.Is("2014-10-20 10:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single();
			@event.Adherence.Should().Be(EventAdherence.In);
		}

		[Test]
		public void ShouldPublishWithOutOfAdherence()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2014-10-20 10:00", "2014-10-20 11:00")
				.WithMappedRule("break", phone, 1);
			Now.Is("2014-10-20 10:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "break"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single();
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
				.WithMappedRule("admin", admin, 0, Adherence.Configuration.Adherence.Neutral);
			Now.Is("2015-03-13 08:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});

			Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single()
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
				.WithStateGroup(null, "default", true);
			Now.Is("2017-03-06 10:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "break"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single();
			@event.ActivityName.Should().Be("phone");
		}

		[Test]
		public void ShouldPublishWithActivityColor()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, Color.Green, "2017-03-06 10:00", "2017-03-06 11:00")
				.WithStateGroup(null, "default", true);
			Now.Is("2017-03-06 10:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "break"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single();
			@event.ActivityColor.Should().Be(Color.Green.ToArgb());
		}

		[Test]
		public void ShouldPublishWithRule()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2017-03-06 10:00", "2017-03-06 11:00")
				.WithMappedRule("break", phone, "out");
			Now.Is("2017-03-06 10:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "break"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single();
			@event.RuleName.Should().Be("out");
		}

		[Test]
		public void ShouldPublishWithRuleColor()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2017-03-06 10:00", "2017-03-06 11:00")
				.WithMappedRule(Guid.NewGuid(), "break", phone, 0, "out", Adherence.Configuration.Adherence.Out, Color.DarkGoldenrod)
				;
			Now.Is("2017-03-06 10:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "break"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single();
			@event.RuleColor.Should().Be(Color.DarkGoldenrod.ToArgb());
		}

		[Test]
		public void ShouldPublishWithOtherRuleColor()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2017-03-06 10:00", "2017-03-06 11:00")
				.WithMappedRule(Guid.NewGuid(), "break", phone, 0, "out", Adherence.Configuration.Adherence.Out, Color.Chocolate)
				;
			Now.Is("2017-03-06 10:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "break"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single();
			@event.RuleColor.Should().Be(Color.Chocolate.ToArgb());
		}

		[Test]
		public void ShouldPublishWithStateName()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2017-03-06 10:00", "2017-03-06 11:00")
				.WithStateGroup(null, "out")
				.WithStateCode("break")
				.WithMappedRule(Guid.NewGuid(), "break", phone, 0, "out", Adherence.Configuration.Adherence.Out)
				;
			Now.Is("2017-03-06 10:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "break"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single();
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
				.WithMappedRule(Guid.NewGuid(), "break", phone, 0, "out", Adherence.Configuration.Adherence.Out)
				;
			Now.Is("2017-03-06 10:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "break"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single();
			@event.StateGroupId.Should().Be(stateGroup);
		}


	}
}