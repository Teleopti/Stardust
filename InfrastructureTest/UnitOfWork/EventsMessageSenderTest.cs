using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldPopEventsFromAggregates()
		{
			var target = new EventsMessageSender(MockRepository.GenerateMock<IEventsPublisher>());
			var root = MockRepository.GenerateMock<IAggregateRootWithEvents>();
			root.Stub(x => x.PopAllEvents()).Return(Enumerable.Empty<IEvent>());
			var roots = new IRootChangeInfo[] { new RootChangeInfo(root, DomainUpdateType.Insert) };

			target.Execute(null, roots);

			root.AssertWasCalled(x => x.PopAllEvents());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldPublishEvents()
		{
			var eventsPublisher = MockRepository.GenerateMock<IEventsPublisher>();
			var target = new EventsMessageSender(eventsPublisher);

			var root = new PersonAbsence(new FakeCurrentScenario().Current());
			var dateTimeperiod =
				new DateOnlyPeriod(DateOnly.Today, DateOnly.Today).ToDateTimePeriod(TimeZoneInfoFactory.UtcTimeZoneInfo());
			root.FullDayAbsence(PersonFactory.CreatePersonWithId(), AbsenceFactory.CreateAbsenceWithId(), dateTimeperiod.StartDateTime, dateTimeperiod.EndDateTime);
			var expected = root.AllEvents();
			var roots = new IRootChangeInfo[] { new RootChangeInfo(root, DomainUpdateType.Insert) };

			target.Execute(null, roots);

			eventsPublisher.AssertWasCalled(x => x.Publish(Arg<IEnumerable<IEvent>>.List.ContainsAll(expected)));
		}
	}
}