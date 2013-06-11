using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
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
			var roots = new IRootChangeInfo[] { new RootChangeInfo(root, DomainUpdateType.Insert) };

			target.Execute(roots);

			root.AssertWasCalled(x => x.PopAllEvents());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldPublishEvents()
		{
			var eventsPublisher = MockRepository.GenerateMock<IEventsPublisher>();
			var target = new EventsMessageSender(eventsPublisher);

			var root = new PersonAbsence(new FakeCurrentScenario().Current());
			root.FullDayAbsence("", PersonFactory.CreatePersonWithId(), AbsenceFactory.CreateAbsenceWithId(), DateTime.Today, DateTime.Today, DateTime.Today, DateTime.Today);
			var expected = root.AllEvents();
			var roots = new IRootChangeInfo[] { new RootChangeInfo(root, DomainUpdateType.Insert) };

			target.Execute(roots);

			eventsPublisher.AssertWasCalled(x => x.Publish(Arg<IEnumerable<IEvent>>.List.ContainsAll(expected)));
		}
	}
}