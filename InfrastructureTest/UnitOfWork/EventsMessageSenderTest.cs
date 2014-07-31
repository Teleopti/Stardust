using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	public class EventsMessageSenderTest
	{
		[Test]
		public void ShouldPopEventsFromAggregates()
		{
			var target = new EventsMessageSender(MockRepository.GenerateMock<IEventsPublisher>());
			var root = MockRepository.GenerateMock<IAggregateRootWithEvents>();
			root.Stub(x => x.PopAllEvents()).Return(Enumerable.Empty<IEvent>());
			var roots = new IRootChangeInfo[] { new RootChangeInfo(root, DomainUpdateType.Insert) };

			target.Execute(roots);

			root.AssertWasCalled(x => x.PopAllEvents());
		}

		[Test]
		public void ShouldPublishEvents()
		{
			var eventsPublisher = new FakeEventsPublisher();
			var target = new EventsMessageSender(eventsPublisher);

			var root = new PersonAbsence(new FakeCurrentScenario().Current());
			var dateTimeperiod =
				new DateOnlyPeriod(DateOnly.Today, DateOnly.Today).ToDateTimePeriod(TimeZoneInfoFactory.UtcTimeZoneInfo());
			root.FullDayAbsence(PersonFactory.CreatePersonWithId(), AbsenceFactory.CreateAbsenceWithId(), dateTimeperiod.StartDateTime, dateTimeperiod.EndDateTime, new TrackedCommandInfo());
			var roots = new IRootChangeInfo[] { new RootChangeInfo(root, DomainUpdateType.Insert) };

			target.Execute(roots);

			eventsPublisher.PublishedEvents.Single().Should().Be.OfType<FullDayAbsenceAddedEvent>();
		}
	}

	public class FakeEventsPublisher : IEventsPublisher
	{
		public IList<IEvent> PublishedEvents = new List<IEvent>();
 
		public void Publish(IEnumerable<IEvent> events)
		{
			events.ForEach(PublishedEvents.Add);
		}
	}
}