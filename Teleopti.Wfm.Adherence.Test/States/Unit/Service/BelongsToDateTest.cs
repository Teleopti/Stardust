using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.Domain.Events;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service
{
	[TestFixture]
	[RtaTest]
	public class BelongsToDateTest
	{
		public FakeDatabase Database;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public Rta Target;
		public FakeExternalLogonReadModelPersister ExternalLogons;

		[Test]
		public void ShouldPublishWithBelongsToDateFromCurrentSchedule()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, null, "2015-02-19", "2015-02-20 1:00", "2015-02-20 7:00")
				.WithMappedRule("phone", phone, 0)
				;
			Now.Is("2015-02-20 2:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>()
				.Single()
				.BelongsToDate.Should().Be("2015-02-19".Date());
		}

		[Test]
		public void ShouldBelongToShiftEnded()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, null, "2015-02-20", "2015-02-20 8:00", "2015-02-20 17:00")
				.WithMappedRule("phone", null, 0)
				;
			Now.Is("2015-02-20 18:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>()
				.Single()
				.BelongsToDate.Should().Be("2015-02-20".Date());
		}

		[Test]
		public void ShouldBelongToShiftStarting()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, null, "2015-02-20", "2015-02-20 8:00", "2015-02-20 17:00")
				.WithMappedRule("phone", null, 0)
				;
			Now.Is("2015-02-20 7:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>()
				.Single()
				.BelongsToDate.Should().Be("2015-02-20".Date());
		}

		[Test]
		public void ShouldPublishOutOfAdherenceEventWithBelongsToDate()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, null, "2015-02-19", "2015-02-20 1:00", "2015-02-20 7:00")
				.WithMappedRule("phone", phone, -1)
				;
			Now.Is("2015-02-20 2:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Publisher.PublishedEvents.OfType<PersonOutOfAdherenceEvent>()
				.Single()
				.BelongsToDate.Should().Be("2015-02-19".Date());
		}

		[Test]
		public void ShouldPublishNeutralAdherenceEventWithBelongsToDate()
		{
			var personId = Guid.NewGuid();
			var admin = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, admin, null, "2015-02-19", "2015-02-20 1:00", "2015-02-20 7:00")
				.WithMappedRule("admin", admin, 0, Adherence.Configuration.Adherence.Neutral)
				;
			Now.Is("2015-02-20 2:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});

			Publisher.PublishedEvents.OfType<PersonNeutralAdherenceEvent>()
				.Single()
				.BelongsToDate.Should().Be("2015-02-19".Date());
		}

		[Test]
		public void ShouldPublishShiftStartEventWithBelongsToDate()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, Guid.NewGuid(), null, "2015-02-19", "2015-02-20 1:00", "2015-02-20 7:00")
				;

			Now.Is("2015-02-20 2:00");
			Target.CheckForActivityChanges(Database.TenantName(), personId);

			Publisher.PublishedEvents.OfType<PersonShiftStartEvent>()
				.Single()
				.BelongsToDate.Should().Be("2015-02-19".Date());
		}

		[Test]
		public void ShouldPublishShiftEndEventWithBelongsToDate()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, Guid.NewGuid(), null, "2015-02-19", "2015-02-20 1:00", "2015-02-20 7:00")
				;

			Now.Is("2015-02-20 2:00");
			Target.CheckForActivityChanges(Database.TenantName(), personId);
			Now.Is("2015-02-20 8:00");
			Target.CheckForActivityChanges(Database.TenantName(), personId);

			Publisher.PublishedEvents.OfType<PersonShiftEndEvent>()
				.Single()
				.BelongsToDate.Should().Be("2015-02-19".Date());
		}

		[Test]
		public void ShouldPublishShiftEndEventWithWithDateOfShiftNotEndedYesterday()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "2015-07-05 10:00", "2015-07-05 11:00")
				.WithSchedule(personId, activityId, "2015-07-06 10:00", "2015-07-06 11:00");

			Now.Is("2015-07-05 10:59");
			Target.CheckForActivityChanges(Database.TenantName(), personId);
			Now.Is("2015-07-06 09:01");
			Target.CheckForActivityChanges(Database.TenantName(), personId);

			var @event = Publisher.PublishedEvents.OfType<PersonShiftEndEvent>().Single();
			@event.BelongsToDate.Should().Be("2015-07-05".Date());
		}

		[Test]
		public void ShouldPublishActivityStartEventWithBelongsToDate()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, Guid.NewGuid(), null, "2015-02-19", "2015-02-20 1:00", "2015-02-20 7:00")
				;

			Now.Is("2015-02-20 2:00");
			Target.CheckForActivityChanges(Database.TenantName(), personId);

			Publisher.PublishedEvents.OfType<PersonActivityStartEvent>()
				.Single()
				.BelongsToDate.Should().Be("2015-02-19".Date());
		}
		
		[Test]
		public void ShouldPublishStateChangedEventWithBelongsToDate()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, null, "2015-02-19", "2015-02-20 1:00", "2015-02-20 7:00")
				.WithStateCode("phone");
			Now.Is("2015-02-20 2:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Publisher.PublishedEvents.OfType<PersonStateChangedEvent>()
				.Single()
				.BelongsToDate.Should().Be("2015-02-19".Date());
		}

		[Test]
		public void ShouldPublishRuleChangedEventWithBelongsToDate()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, null, "2015-02-19", "2015-02-20 1:00", "2015-02-20 7:00")
				.WithMappedRule(Guid.NewGuid(), "break", phone, 0, "out", Adherence.Configuration.Adherence.Out);
			Now.Is("2015-02-20 2:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Publisher.PublishedEvents.OfType<PersonRuleChangedEvent>()
				.Single()
				.BelongsToDate.Should().Be("2015-02-19".Date());
		}


		[Test]
		public void ShouldPublishPersonAdherenceDayStartEventWithBelongsToDate()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "2018-10-25 10:00", "2018-10-25 11:00");

			Now.Is("2018-10-25 09:00");
			Target.CheckForActivityChanges(Database.TenantName());

			var @event = Publisher.PublishedEvents.OfType<PersonAdherenceDayStartEvent>().Single();
			@event.BelongsToDate.Should().Be("2018-10-25".Date());
		}

		[Test]
		public void ShouldBelongToNextShiftWhenNear2Shifts()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "2018-10-24", "2018-10-24 23:00", "2018-10-25 07:00")
				.WithSchedule(personId, activityId, "2018-10-25", "2018-10-25 08:00", "2018-10-25 17:00")
				.WithStateGroup(null, "default", true)
				;

			Now.Is("2018-10-25 07:01");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single().BelongsToDate
				.Should().Be("2018-10-25".Date());
		}

		[Test]
		public void ShouldBelongToAgentsDateInChinaWhenNotNearPastShift()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithAgent(personId, "usercode", TimeZoneInfoFactory.ChinaTimeZoneInfo())
				.WithSchedule(personId, activityId, "2018-10-25", "2018-10-25 08:00", "2018-10-25 17:00")
				.WithStateGroup(null, "default", true)
				;

			Now.Is("2018-10-25 18:01");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single().BelongsToDate
				.Should().Be("2018-10-26".Date());
		}

		[Test]
		public void ShouldBelongToAgentsDateInAustraliaWhenNotNearPastShift()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithAgent(personId, "usercode", TimeZoneInfoFactory.AustralianTimeZoneInfo())
				.WithSchedule(personId, activityId, "2018-10-25", "2018-10-25 08:00", "2018-10-25 13:00")
				.WithStateGroup(null, "default", true)
				;

			Now.Is("2018-10-25 14:01");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single().BelongsToDate
				.Should().Be("2018-10-26".Date());
		}

		[Test]
		public void ShouldNotBreakWithNullTimeZone()
		{
			ExternalLogons.Add(new ExternalLogonReadModel
			{
				DataSourceId = Database.CurrentDataSourceId(),
				PersonId = Guid.NewGuid(),
				TimeZone = null,
				UserCode = "usercode"
			});
			ExternalLogons.Refresh();
			Database.WithAgent("usercode");

			Assert.DoesNotThrow(() => { Target.CheckForActivityChanges(Database.TenantName()); });
		}

		[Test]
		public void ShouldNotBreakWithIncorrectTimeZone()
		{
			ExternalLogons.Add(new ExternalLogonReadModel
			{
				DataSourceId = Database.CurrentDataSourceId(),
				PersonId = Guid.NewGuid(),
				TimeZone = "incorrect",
				UserCode = "usercode"
			});
			ExternalLogons.Refresh();
			Database.WithAgent("usercode");

			Assert.DoesNotThrow(() => { Target.CheckForActivityChanges(Database.TenantName()); });
		}
		
		[Test]
		public void ShouldPublishArrivedLateForWorkEventWithBelongsToDate()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2018-10-31 08:00", "2018-10-31 17:00")
				.WithStateGroup("Phone").WithStateCode("phone")
				.WithLoggedOutStateGroup("Logged Out").WithStateCode("loggedOut")
				;
			Now.Is("2018-10-30 17:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "loggedOut"
			});
			Now.Is("2018-10-31 08:30");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Publisher.PublishedEvents.OfType<PersonArrivedLateForWorkEvent>().Single().BelongsToDate
				.Should().Be("2018-10-31".Date());
		}		
	}
}