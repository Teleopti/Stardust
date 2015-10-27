using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork.PersistCallbacks.ImplementationDetails
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

			target.AfterFlush(roots);

			root.AssertWasCalled(x => x.PopAllEvents());
		}

		[Test]
		public void ShouldPublishEvents()
		{
			var eventsPublisher = new FakeEventsPublisher();
			var target = new EventsMessageSender(eventsPublisher);

			var absence = new Absence();
			
			var startDateTime = new DateTime(2015, 10, 1, 8, 0, 0, DateTimeKind.Utc);
			var absenceLayer = new AbsenceLayer(absence, new DateTimePeriod(startDateTime, startDateTime.AddHours(8)));

			var root = new PersonAbsence(new Person(), new FakeCurrentScenario().Current(), absenceLayer);

			root.FullDayAbsence(PersonFactory.CreatePersonWithId(), null);
			var roots = new IRootChangeInfo[] { new RootChangeInfo(root, DomainUpdateType.Insert) };

			target.AfterFlush(roots);

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