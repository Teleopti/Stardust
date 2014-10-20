using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;

namespace Teleopti.Ccc.WebTest.Areas.Rta.Core
{
	[TestFixture]
	public class PersonInAdherenceEventTest
	{
		[Test]
		public void ShouldPublishPersonInAdherenceEvent() // (...WhenNoStaffingEffect)
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode",
				Timestamp = new DateTime(2014, 10, 20, 9, 0, 0, DateTimeKind.Utc)
			};
			var personId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.AddSource(state.SourceId)
				.AddUser("usercode", personId, Guid.NewGuid())
				.AddSchedule(state.Timestamp.AddHours(-1), state.Timestamp.AddHours(1))
				.Done();
			var publisher = new FakeEventsPublisher();
			var target = new TeleoptiRtaServiceForTest(database, new ThisIsNow(state.Timestamp), publisher);

			target.SaveExternalUserState(state);

			var @event = publisher.PublishedEvents.Single() as PersonInAdherenceEvent;
			@event.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldNotPublishPersonInAdherenceEventWhenNoSchedule() 
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode",
				Timestamp = new DateTime(2014, 10, 20, 9, 0, 0, DateTimeKind.Utc)
			};
			var personId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.AddSource(state.SourceId)
				.AddUser("usercode", personId, Guid.NewGuid())
				.Done();
			var publisher = new FakeEventsPublisher();
			var target = new TeleoptiRtaServiceForTest(database, new ThisIsNow(state.Timestamp), publisher);

			target.SaveExternalUserState(state);

			publisher.PublishedEvents.Should().Have.Count.EqualTo(0);
		}

	}
}