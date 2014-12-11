using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	[TestFixture]
	public class PersonActivityStartEventTest
	{
		[Test]
		public void ShouldPublishEvent()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.Make();
			var publisher = new FakeEventPublisher();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:00".Utc()), publisher);
			
			target.CheckForActivityChange(personId, businessUnitId);

			var @event = publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single();
			@event.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldPublishIfNextActivityHasStarted()
		{
			var personId = Guid.NewGuid();
			var activityId1 = Guid.NewGuid();
			var activityId2 = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId1, "2014-10-20 10:00", "2014-10-20 10:15")
				.WithSchedule(personId, activityId2, "2014-10-20 10:15", "2014-10-20 11:00")
				.Make();
			var publisher = new FakeEventPublisher();
			var now = new MutableNow();
			now.Mutate("2014-10-20 10:00");
			var target = new RtaForTest(database, now, publisher);

			target.CheckForActivityChange(personId, businessUnitId);
			now.Mutate("2014-10-20 10:05");
			target.CheckForActivityChange(personId, businessUnitId);
			now.Mutate("2014-10-20 10:15");
			target.CheckForActivityChange(personId, businessUnitId);

			var events = publisher.PublishedEvents.OfType<PersonActivityStartEvent>();
			events.Should().Have.Count.EqualTo(2);
		}

		[Test]
		public void ShouldPublishWithActivityInfo()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId, "phone", "2014-10-20 10:00", "2014-10-20 11:00")
				.Make();
			var publisher = new FakeEventPublisher();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:02".Utc()), publisher);

			target.CheckForActivityChange(personId, businessUnitId);

			var @event = publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single();
			@event.StartTime.Should().Be("2014-10-20 10:00".Utc());
			@event.Name.Should().Be("phone");
		}

		[Test]
		public void ShouldPublishWithLogOnInfo()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.Make();
			var publisher = new FakeEventPublisher();
			var dataSource = new FakeCurrentDatasource("datasource");
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:00".Utc()), publisher, dataSource);

			target.CheckForActivityChange(personId, businessUnitId);

			var @event = (ILogOnInfo)publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single();
			@event.BusinessUnitId.Should().Be(businessUnitId);
			@event.Datasource.Should().Be("datasource");
		}

		[Test]
		public void ShouldPublishWithInAdherence()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithBusinessUnit(businessUnitId)
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, phone, "2014-10-20 10:00", "2014-10-20 11:00")
				.WithAlarm("phone", phone, 0)
				.Make();
			var publisher = new FakeEventPublisher();
			var mutableNow = new MutableNow();
			mutableNow.Mutate("2014-10-20 09:50");
			var target = new RtaForTest(database, mutableNow, publisher);

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			mutableNow.Mutate("2014-10-20 10:02");
			target.CheckForActivityChange(personId, businessUnitId);

			var @event = publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single();
			@event.InAdherence.Should().Be(true);
		}

		[Test]
		public void ShouldPublishWithOutAdherence()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithBusinessUnit(businessUnitId)
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId, "phone", "2014-10-20 10:00", "2014-10-20 11:00")
				.WithAlarm("statecode", activityId, 1d)
				.Make();
			var publisher = new FakeEventPublisher();
			var mutableNow = new MutableNow();
			mutableNow.Mutate("2014-10-20 09:50");
			var target = new RtaForTest(database, mutableNow, publisher);

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});
			mutableNow.Mutate("2014-10-20 10:02");
			target.CheckForActivityChange(personId, businessUnitId);

			var @event = publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single();
			@event.InAdherence.Should().Be(false);
		}

	}
}