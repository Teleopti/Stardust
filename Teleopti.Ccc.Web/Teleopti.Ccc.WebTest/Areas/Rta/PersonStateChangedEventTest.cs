using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	[TestFixture]
	public class PersonStateChangedEventTest
	{
		[Test]
		public void ShouldPublishEvent()
		{
			var personId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId)
				.Make();
			var publisher = new FakeEventPublisher();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:00"), publisher);

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			var @event = publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single();
			@event.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldPublishEventOnlyIfStateChanged()
		{
			var personId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId)
				.WithAlarm("stateone", Guid.NewGuid())
				.WithAlarm("statetwo", Guid.NewGuid())
				.Make();
			var publisher = new FakeEventPublisher();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:00"), publisher);

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "stateone"
			});
			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "stateone"
			});
			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statetwo"
			});

			var events = publisher.PublishedEvents.OfType<PersonStateChangedEvent>();
			events.Should().Have.Count.EqualTo(2);
		}

		[Test]
		public void ShouldPublishWithSystemTime()
		{
			var personId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId)
				.Make();
			var publisher = new FakeEventPublisher();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:00"), publisher);

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			var @event = publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single();
			@event.Timestamp.Should().Be("2014-10-20 10:00".Utc());
		}

		[Test]
		public void ShouldPublishWithLogOnInfo()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.Make();
			var publisher = new FakeEventPublisher();
			var dataSource = new FakeCurrentDatasource("datasource");
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:00"), publisher, dataSource);

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			var @event = (ILogOnInfo)publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single();
			@event.BusinessUnitId.Should().Be(businessUnitId);
			@event.Datasource.Should().Be("datasource");
		}

		[Test]
		public void ShouldPublishWithInAdherence()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.WithAlarm("statecode", activityId, 0)
				.Make();
			var publisher = new FakeEventPublisher();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:00"), publisher);

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			var @event = publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single();
			@event.InAdherence.Should().Be(true);
		}

		[Test]
		public void ShouldPublishWithOutOfAdherence()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2014-10-20 10:00", "2014-10-20 11:00")
				.WithAlarm("break", phone, 1)
				.Make();
			var publisher = new FakeEventPublisher();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:00"), publisher);

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "break"
			});

			var @event = publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single();
			@event.InAdherence.Should().Be(false);
		}

		[Test]
		public void ShouldPublishInAdherenceForPreviousActivity()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.WithAlarm("phone", activityId, 0)
				.Make();
			var publisher = new FakeEventPublisher();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 11:05"), publisher);

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			var @event = publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single();
			@event.InAdherenceWithPreviousActivity.Should().Be(true);
		}

		[Test]
		public void ShouldPublishOutOfAdherenceForPreviousActivity()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.WithAlarm("phone", activityId, 1)
				.Make();
			var publisher = new FakeEventPublisher();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 11:05"), publisher);

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			var @event = publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single();
			@event.InAdherenceWithPreviousActivity.Should().Be(false);
		}

	}
}