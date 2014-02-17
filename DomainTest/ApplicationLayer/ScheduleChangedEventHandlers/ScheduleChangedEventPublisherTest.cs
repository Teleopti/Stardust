﻿using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers
{
	[TestFixture]
	public class ScheduleChangedEventPublisherTest
	{
		[Test]
		public void ShouldPublishScheduleChangedEventOnActivityAssignedEvent()
		{
			var publisher = new FakePublishEventsFromEventHandlers();
			var target = new ScheduleChangedEventPublisher(publisher);

			var @event = new ActivityAddedEvent
				{
					Timestamp = new DateTime(2013, 11, 15, 10, 0, 0),
					Datasource = "datasource",
					BusinessUnitId = Guid.NewGuid(),
					PersonId = Guid.NewGuid(),
					Date = new DateOnly(2013,11,15),
					ActivityId = Guid.NewGuid(),
					ScenarioId = Guid.NewGuid(),
					StartDateTime = new DateTime(2013, 11, 15, 8, 0, 0),
					EndDateTime = new DateTime(2013, 11, 15, 9, 0, 0)
				};
			target.Handle(@event);

			var published = publisher.Published<ScheduleChangedEvent>();
			published.Timestamp.Should().Be(@event.Timestamp);
			published.Datasource.Should().Be(@event.Datasource);
			published.BusinessUnitId.Should().Be(@event.BusinessUnitId);
			published.PersonId.Should().Be(@event.PersonId);
			published.ScenarioId.Should().Be(@event.ScenarioId);
			published.StartDateTime.Should().Be(@event.StartDateTime);
			published.EndDateTime.Should().Be(@event.EndDateTime);
		}
	}
}