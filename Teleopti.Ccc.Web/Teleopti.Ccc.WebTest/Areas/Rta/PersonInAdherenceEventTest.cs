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
				Timestamp = new DateTime(2014, 10, 20, 9, 0, 0, DateTimeKind.Utc)
			};
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, state.Timestamp.AddHours(-1), state.Timestamp.AddHours(1))
				.WithAlarm("statecode", activityId, 0)
				.Make();
			var publisher = new FakeEventsPublisher();
			var target = new RtaForTest(database, new ThisIsNow(state.Timestamp), publisher);

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
				Timestamp = new DateTime(2014, 10, 20, 9, 0, 0, DateTimeKind.Utc)
			};
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, state.Timestamp.AddHours(-1), state.Timestamp.AddHours(1))
				.Make();
			var publisher = new FakeEventsPublisher();
			var target = new RtaForTest(database, new ThisIsNow(state.Timestamp), publisher);

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
				StateCode = "statecode1",
				Timestamp = new DateTime(2014, 10, 20, 9, 0, 0, DateTimeKind.Utc)
			};
			var state2 = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode2",
				Timestamp = new DateTime(2014, 10, 20, 9, 0, 0, DateTimeKind.Utc)
			};
			var activityId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state1)
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, state1.Timestamp.AddHours(-1), state1.Timestamp.AddHours(1))
				.WithAlarm("statecode1", activityId, 0)
				.WithAlarm("statecode2", activityId, 0)
				.Make();
			var publisher = new FakeEventsPublisher();
			var target = new RtaForTest(database, new ThisIsNow(state1.Timestamp), publisher);

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
				StateCode = "statecode",
				Timestamp = new DateTime(2014, 10, 20, 9, 0, 0, DateTimeKind.Utc)
			};
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, state.Timestamp.AddHours(-1), state.Timestamp.AddHours(1))
				.WithAlarm("statecode", activityId, 0)
				.Make();
			var publisher = new FakeEventsPublisher();
			var target = new RtaForTest(database, new ThisIsNow(state.Timestamp), publisher);

			target.SaveState(state);

			var @event = publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Single();
			@event.Timestamp.Should().Be(state.Timestamp);
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
				.WithSchedule(personId, activityId, "2014-11-11 10:00".Utc(), "2014-11-11 12:00".Utc())
				.WithAlarm("statecode", activityId, 0)
				.Make();
			var publisher = new FakeEventsPublisher();
			var dataSource = new FakeCurrentDatasource("datasource");
			var target = new RtaForTest(database, new ThisIsNow("2014-11-11 11:00"), publisher, dataSource);

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode",
				Timestamp = "2014-11-11 11:00".Utc()
			});

			var @event = (ILogOnInfo) publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Single();
			@event.BusinessUnitId.Should().Be(businessUnitId);
			@event.Datasource.Should().Be("datasource");
		}

	}
}