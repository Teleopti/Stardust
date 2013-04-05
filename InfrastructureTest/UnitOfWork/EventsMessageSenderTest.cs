using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
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

			var scenario = new Scenario(" ");
			scenario.SetId(new Guid());
			var root = new PersonAbsence(scenario);

			var person = new Person();
			person.SetId(new Guid());

			var absence = new Absence();
			absence.SetId(new Guid());

			root.FullDayAbsence(person, absence, DateTime.Today, DateTime.Today);
			var expected = root.AllEvents();
			var roots = new IRootChangeInfo[] { new RootChangeInfo(root, DomainUpdateType.Insert) };

			target.Execute(roots);

			eventsPublisher.AssertWasCalled(x => x.Publish(Arg<IEnumerable<IEvent>>.List.ContainsAll(expected)));
		}
	}
}