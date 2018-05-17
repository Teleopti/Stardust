﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers
{
	[TestFixture]
	[DomainTest]
	public class ScheduleChangedEventPublisherTest
	{
		public ScheduleChangedEventPublisher Target;
		public FakeEventPublisher Publisher;

		[Test]
		public void ShouldPublishScheduleChangedEventOnActivityAssignedEvent()
		{
			var @event = new ActivityAddedEvent
				{
					Timestamp = new DateTime(2013, 11, 15, 10, 0, 0),
					LogOnDatasource = "datasource",
					LogOnBusinessUnitId = Guid.NewGuid(),
					PersonId = Guid.NewGuid(),
					Date = new DateTime(2013,11,15),
					ActivityId = Guid.NewGuid(),
					ScenarioId = Guid.NewGuid(),
					StartDateTime = new DateTime(2013, 11, 15, 8, 0, 0),
					EndDateTime = new DateTime(2013, 11, 15, 9, 0, 0)
				};
			Target.Handle(@event);

			var published = Publisher.PublishedEvents.OfType<ScheduleChangedEvent>().Single();
			published.Timestamp.Should().Be(@event.Timestamp);
			published.LogOnDatasource.Should().Be(@event.LogOnDatasource);
			published.LogOnBusinessUnitId.Should().Be(@event.LogOnBusinessUnitId);
			published.PersonId.Should().Be(@event.PersonId);
			published.ScenarioId.Should().Be(@event.ScenarioId);
			published.StartDateTime.Should().Be(@event.StartDateTime);
			published.EndDateTime.Should().Be(@event.EndDateTime);
		}

		[Test]
		public void ShouldPassDateWhenActivityAddedEventHasFired()
		{
			var @event = new ActivityAddedEvent
			{
				Timestamp = new DateTime(2013, 11, 15, 10, 0, 0),
				LogOnDatasource = "datasource",
				LogOnBusinessUnitId = Guid.NewGuid(),
				PersonId = Guid.NewGuid(),
				Date = new DateTime(2013, 11, 15),
				ActivityId = Guid.NewGuid(),
				ScenarioId = Guid.NewGuid(),
				StartDateTime = new DateTime(2013, 11, 15, 8, 0, 0),
				EndDateTime = new DateTime(2013, 11, 15, 9, 0, 0)
			};
			Target.Handle(@event);

			var published = Publisher.PublishedEvents.OfType<ScheduleChangedEvent>().Single();
			published.Date.HasValue.Should().Be(true);
			published.Date.Value.Should().Be(@event.Date);

		}

		[Test]
		public void ShouldPassDateWhenPersonAssignmentLayerRemovedEventHasFired()
		{
			var @event = new PersonAssignmentLayerRemovedEvent
			{
				Timestamp = new DateTime(2013, 11, 15, 10, 0, 0),
				LogOnDatasource = "datasource",
				LogOnBusinessUnitId = Guid.NewGuid(),
				PersonId = Guid.NewGuid(),
				Date = new DateTime(2013, 11, 15),
				ScenarioId = Guid.NewGuid(),
				StartDateTime = new DateTime(2013, 11, 15, 8, 0, 0),
				EndDateTime = new DateTime(2013, 11, 15, 9, 0, 0)
			};
			Target.Handle(@event);

			var published = Publisher.PublishedEvents.OfType<ScheduleChangedEvent>().Single();
			published.Date.HasValue.Should().Be(true);
			published.Date.Value.Should().Be(@event.Date);
		}

		[Test]
		public void ShouldPublishScheduleChangedEventWhenActivityMovedEventHasFired()
		{
			var theEvent = new ActivityMovedEvent
			{
				Timestamp = DateTime.Now,
				LogOnDatasource = "datasource",
				LogOnBusinessUnitId = Guid.NewGuid(),
				PersonId = Guid.NewGuid(),
				ScenarioId = Guid.NewGuid(),
				StartDateTime = DateTime.Now,
				EndDateTime = DateTime.Now.AddHours(2)
			};
			Target.Handle(theEvent);

			var published = Publisher.PublishedEvents.OfType<ScheduleChangedEvent>().Single();
			published.Timestamp.Should().Be(theEvent.Timestamp);
			published.LogOnDatasource.Should().Be(theEvent.LogOnDatasource);
			published.LogOnBusinessUnitId.Should().Be(theEvent.LogOnBusinessUnitId);
			published.PersonId.Should().Be(theEvent.PersonId);
			published.ScenarioId.Should().Be(theEvent.ScenarioId);
			published.StartDateTime.Should().Be(theEvent.StartDateTime);
			published.EndDateTime.Should().Be(theEvent.EndDateTime);
		}

		[Test]
		public void ShouldPublishScheduleChangedEventWhenMainShiftCategoryReplaceEventHasFired()
		{
			var theEvent = new MainShiftCategoryReplaceEvent
			{
				Timestamp = DateTime.Now,
				LogOnDatasource = "datasource",
				LogOnBusinessUnitId = Guid.NewGuid(),
				PersonId = Guid.NewGuid(),
				ScenarioId = Guid.NewGuid(),
				Date = DateTime.Today,
				CommandId = Guid.NewGuid()
			};
			Target.Handle(theEvent);

			var published = Publisher.PublishedEvents.OfType<ScheduleChangedEvent>().Single();
			published.Timestamp.Should().Be(theEvent.Timestamp);
			published.LogOnDatasource.Should().Be(theEvent.LogOnDatasource);
			published.LogOnBusinessUnitId.Should().Be(theEvent.LogOnBusinessUnitId);
			published.PersonId.Should().Be(theEvent.PersonId);
			published.ScenarioId.Should().Be(theEvent.ScenarioId);
			published.StartDateTime.Should().Be(theEvent.Date.AddDays(-1));
			published.EndDateTime.Should().Be(theEvent.Date.AddDays(2));
			published.CommandId.Should().Be(theEvent.CommandId);
		}
	}
}