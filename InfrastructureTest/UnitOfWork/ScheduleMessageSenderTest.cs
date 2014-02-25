using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
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
	public class ScheduleMessageSenderTest
	{
		[Test]
		public void ShouldSendNotificationForPersistableScheduleData()
		{
			var serviceBusSender = MockRepository.GenerateMock<IServiceBusSender>();
			var beforeSendEvents = MockRepository.GenerateMock<IBeforeSendEvents>();
			var person = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, scenario);
			IRootChangeInfo rootChangeInfo = new RootChangeInfo(personAssignment, DomainUpdateType.Insert);
			var target = new ScheduleMessageSender(new ServiceBusEventPublisher(serviceBusSender, new DummyContextPopulator()), beforeSendEvents);
			serviceBusSender.Stub(x => x.EnsureBus()).Return(true);

			target.Execute(new[] { rootChangeInfo });

			serviceBusSender.AssertWasCalled(x => x.Send(null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldCallHookBeforeSendNotificationForPersistableScheduleData()
		{
			var serviceBusSender = MockRepository.GenerateMock<IServiceBusSender>();
			var beforeSendEvents = MockRepository.GenerateMock<IBeforeSendEvents>();
			var person = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, scenario);
			IRootChangeInfo rootChangeInfo = new RootChangeInfo(personAssignment, DomainUpdateType.Insert);
			var target = new ScheduleMessageSender(new ServiceBusEventPublisher(serviceBusSender, new DummyContextPopulator()), beforeSendEvents);
			serviceBusSender.Stub(x => x.EnsureBus()).Return(true);

			target.Execute(new[] {rootChangeInfo});

			beforeSendEvents.AssertWasCalled(x => x.Execute(null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldNotSendNotificationForNullScenario()
		{
			var serviceBusSender = MockRepository.GenerateMock<IServiceBusSender>();
			var beforeSendEvents = MockRepository.GenerateMock<IBeforeSendEvents>();
			var person = PersonFactory.CreatePerson();
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, null);
			IRootChangeInfo rootChangeInfo = new RootChangeInfo(personAssignment, DomainUpdateType.Insert);
			var target = new ScheduleMessageSender(new ServiceBusEventPublisher(serviceBusSender, new DummyContextPopulator()), beforeSendEvents);

            serviceBusSender.Stub(x => x.EnsureBus()).Return(true);

			target.Execute(new[] {rootChangeInfo});

			serviceBusSender.AssertWasNotCalled(x => x.Send(null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldNotSendNotificationForOtherType()
		{
			var serviceBusSender = MockRepository.GenerateMock<IServiceBusSender>();
			var beforeSendEvents = MockRepository.GenerateMock<IBeforeSendEvents>();
			var person = PersonFactory.CreatePerson();
			IRootChangeInfo rootChangeInfo = new RootChangeInfo(person, DomainUpdateType.Insert);
			var target = new ScheduleMessageSender(new ServiceBusEventPublisher(serviceBusSender, new DummyContextPopulator()), beforeSendEvents);

            serviceBusSender.Stub(x => x.EnsureBus()).Return(true);

			target.Execute(new[] {rootChangeInfo});
			
			serviceBusSender.AssertWasNotCalled(x => x.Send(null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldNotSendNotificationForInternalNote()
		{
			var serviceBusSender = MockRepository.GenerateMock<IServiceBusSender>();
			var beforeSendEvents = MockRepository.GenerateMock<IBeforeSendEvents>();
			var person = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var note = new Note(person, DateOnly.Today, scenario, "my note");
			IRootChangeInfo rootChangeInfo = new RootChangeInfo(note, DomainUpdateType.Insert);
			var target = new ScheduleMessageSender(new ServiceBusEventPublisher(serviceBusSender, new DummyContextPopulator()), beforeSendEvents);

            serviceBusSender.Stub(x => x.EnsureBus()).Return(true);

			target.Execute(new[] {rootChangeInfo});

			serviceBusSender.AssertWasNotCalled(x => x.Send(null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldNotSendNotificationForPublicNote()
		{
			var serviceBusSender = MockRepository.GenerateMock<IServiceBusSender>();
			var beforeSendEvents = MockRepository.GenerateMock<IBeforeSendEvents>();
			var person = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var note = new PublicNote(person, DateOnly.Today, scenario, "my note");
			IRootChangeInfo rootChangeInfo = new RootChangeInfo(note, DomainUpdateType.Insert);
			var target = new ScheduleMessageSender(new ServiceBusEventPublisher(serviceBusSender, new DummyContextPopulator()), beforeSendEvents);

            serviceBusSender.Stub(x => x.EnsureBus()).Return(true);

			target.Execute(new[] {rootChangeInfo});

			serviceBusSender.AssertWasNotCalled(x => x.Send(null), o => o.IgnoreArguments());
		}

        [Test]
        public void ShouldNotSendNotificationForScheduleDayTag()
        {
			var serviceBusSender = MockRepository.GenerateMock<IServiceBusSender>();
			var beforeSendEvents = MockRepository.GenerateMock<IBeforeSendEvents>();
			var person = PersonFactory.CreatePerson();
            var scenario = ScenarioFactory.CreateScenarioAggregate();
            var agentDayScheduleTag = new AgentDayScheduleTag(person, DateOnly.Today, scenario, new ScheduleTag());
            IRootChangeInfo rootChangeInfo = new RootChangeInfo(agentDayScheduleTag, DomainUpdateType.Insert);
			var target = new ScheduleMessageSender(new ServiceBusEventPublisher(serviceBusSender, new DummyContextPopulator()), beforeSendEvents);

            serviceBusSender.Stub(x => x.EnsureBus()).Return(true);

            target.Execute(new[] { rootChangeInfo });

            serviceBusSender.AssertWasNotCalled(x => x.Send(null), o => o.IgnoreArguments());
        }

		[Test]
		public void ShouldSendMessageBrokerIdentifierWithEvent()
		{
			var serviceBusSender = MockRepository.GenerateMock<IServiceBusSender>();
			var beforeSendEvents = MockRepository.GenerateMock<IBeforeSendEvents>();
			var person = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, scenario);
			IRootChangeInfo rootChangeInfo = new RootChangeInfo(personAssignment, DomainUpdateType.Insert);
			serviceBusSender.Stub(x => x.EnsureBus()).Return(true);
			var initiatorIdentifier = new FakeInitiatorIdentifier { InitiatorId = Guid.NewGuid() };
			var target = new ScheduleMessageSender(new ServiceBusEventPublisher(serviceBusSender, new EventContextPopulator(null, new FakeCurrentInitiatorIdentifier(initiatorIdentifier))), beforeSendEvents);

			target.Execute(new[] { rootChangeInfo });

			serviceBusSender.AssertWasCalled(x => x.Send(Arg<object>.Matches(e =>
				((ScheduleChangedEvent)e).InitiatorId == initiatorIdentifier.InitiatorId
				)));
		}

		[Test]
		public void Execute_WhenTheSchedulesHasChanged_ShouldSetTheStartDateTimeForTheMessageToTheStartTimeOfTheChangedSchedule()
		{
			var serviceBusSender = MockRepository.GenerateMock<IServiceBusSender>();
			var beforeSendEvents = MockRepository.GenerateMock<IBeforeSendEvents>();
			var person = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, scenario);
			IRootChangeInfo rootChangeInfo = new RootChangeInfo(personAssignment, DomainUpdateType.Update);
			serviceBusSender.Stub(x => x.EnsureBus()).Return(true);
			var publisher = new eventPublisherProbe();
			var target = new ScheduleMessageSender(publisher, beforeSendEvents);
			target.Execute(new[] { rootChangeInfo });

			Assert.That(publisher.PublishedEvent.StartDateTime, Is.EqualTo(personAssignment.Period.StartDateTime));
		}

		private class eventPublisherProbe : IEventPublisher
		{
			public ScheduleChangedEvent PublishedEvent;

			public void Publish(IEvent @event)
			{
				PublishedEvent = (ScheduleChangedEvent) @event;
			}

		}
	}
}