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
	public class PersonInAdherenceEventTest
	{
		[Test]
		public void ShouldPublishPersonInAdherenceEventWhenNoStaffingEffect()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode",
			};
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithAlarm("statecode", activityId, 0)
				.Make();
			var publisher = new FakeEventPublisher();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 9:00"), publisher);

			target.SaveState(state);

			var @event = publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Single();
			@event.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldPublishPersonInAdherenceEventWhenNoAlarm()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode",
			};
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 8:00", "2014-10-20 10:00")
				.Make();
			var publisher = new FakeEventPublisher();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 9:00"), publisher);

			target.SaveState(state);

			var @event = publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Single();
			@event.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldNotPublishEventIfStillInAdherence()
		{
			var state1 = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode1"
			};
			var state2 = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode2"
			};
			var activityId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state1)
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithAlarm("statecode1", activityId, 0)
				.WithAlarm("statecode2", activityId, 0)
				.Make();
			var publisher = new FakeEventPublisher();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 9:00"), publisher);

			target.SaveState(state1);
			target.SaveState(state2);

			publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPublishTheTimeWhenPersonInAdherence()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			};
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 9:00", "2014-10-20 10:00")
				.WithAlarm("statecode", activityId, 0)
				.Make();
			var publisher = new FakeEventPublisher();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 9:00"), publisher);

			target.SaveState(state);

			var @event = publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Single();
			@event.Timestamp.Should().Be("2014-10-20 9:00".Utc());
		}

		[Test]
		public void ShouldPublishEventWithLogOnInfo()
		{
			var businessUnitId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithBusinessUnit(businessUnitId)
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-11-11 10:00", "2014-11-11 12:00")
				.WithAlarm("statecode", activityId, 0)
				.Make();
			var publisher = new FakeEventPublisher();
			var dataSource = new FakeCurrentDatasource("datasource");
			var target = new RtaForTest(database, new ThisIsNow("2014-11-11 11:00"), publisher, dataSource);

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			var @event = (ILogOnInfo) publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Single();
			@event.BusinessUnitId.Should().Be(businessUnitId);
			@event.Datasource.Should().Be("datasource");
		}

	}
}