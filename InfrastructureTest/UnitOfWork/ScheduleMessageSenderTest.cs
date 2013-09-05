using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	public class ScheduleMessageSenderTest
	{
		private IMessageSender target;
		private IServiceBusSender serviceBusSender;

		[SetUp]
		public void Setup()
		{
			serviceBusSender = MockRepository.GenerateMock<IServiceBusSender>();
			target = new ScheduleMessageSender(serviceBusSender);
		}

		[Test]
		public void ShouldSendNotificationForPersistableScheduleData()
		{
			var person = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, scenario);
			IRootChangeInfo rootChangeInfo = new RootChangeInfo(personAssignment, DomainUpdateType.Insert);

			serviceBusSender.Stub(x => x.EnsureBus()).Return(true);

			target.Execute(new FakeMessageBrokerIdentifier(), new[] {rootChangeInfo});

			serviceBusSender.AssertWasCalled(x => x.Send(null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldNotSendNotificationForNullScenario()
		{
			var person = PersonFactory.CreatePerson();
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, null);
			IRootChangeInfo rootChangeInfo = new RootChangeInfo(personAssignment, DomainUpdateType.Insert);

            serviceBusSender.Stub(x => x.EnsureBus()).Return(true);

			target.Execute(null, new[] {rootChangeInfo});

			serviceBusSender.AssertWasNotCalled(x => x.Send(null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldNotSendNotificationForOtherType()
		{
			var person = PersonFactory.CreatePerson();
			IRootChangeInfo rootChangeInfo = new RootChangeInfo(person, DomainUpdateType.Insert);

            serviceBusSender.Stub(x => x.EnsureBus()).Return(true);

			target.Execute(null, new[] {rootChangeInfo});
			
			serviceBusSender.AssertWasNotCalled(x => x.Send(null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldNotSendNotificationForInternalNote()
		{
			var person = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var note = new Note(person, DateOnly.Today, scenario, "my note");
			IRootChangeInfo rootChangeInfo = new RootChangeInfo(note, DomainUpdateType.Insert);

            serviceBusSender.Stub(x => x.EnsureBus()).Return(true);

			target.Execute(null, new[] {rootChangeInfo});

			serviceBusSender.AssertWasNotCalled(x => x.Send(null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldNotSendNotificationForPublicNote()
		{
			var person = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var note = new PublicNote(person, DateOnly.Today, scenario, "my note");
			IRootChangeInfo rootChangeInfo = new RootChangeInfo(note, DomainUpdateType.Insert);

            serviceBusSender.Stub(x => x.EnsureBus()).Return(true);

			target.Execute(null, new[] {rootChangeInfo});

			serviceBusSender.AssertWasNotCalled(x => x.Send(null), o => o.IgnoreArguments());
		}

        [Test]
        public void ShouldNotSendNotificationForScheduleDayTag()
        {
            var person = PersonFactory.CreatePerson();
            var scenario = ScenarioFactory.CreateScenarioAggregate();
            var agentDayScheduleTag = new AgentDayScheduleTag(person, DateOnly.Today, scenario, new ScheduleTag());
            IRootChangeInfo rootChangeInfo = new RootChangeInfo(agentDayScheduleTag, DomainUpdateType.Insert);

            serviceBusSender.Stub(x => x.EnsureBus()).Return(true);

            target.Execute(null, new[] { rootChangeInfo });

            serviceBusSender.AssertWasNotCalled(x => x.Send(null), o => o.IgnoreArguments());
        }

		[Test]
		public void ShouldSendMessageBrokerIdentifierWithEvent()
		{
			var person = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, scenario);
			IRootChangeInfo rootChangeInfo = new RootChangeInfo(personAssignment, DomainUpdateType.Insert);
			serviceBusSender.Stub(x => x.EnsureBus()).Return(true);
			var messageBrokerIdentifier = new FakeMessageBrokerIdentifier {InstanceId = Guid.NewGuid()};

			target.Execute(messageBrokerIdentifier, new[] {rootChangeInfo});

			serviceBusSender.AssertWasCalled(x => x.Send(Arg<object>.Matches(e => 
				((ScheduleChangedEvent) e).InitiatorId == messageBrokerIdentifier.InstanceId
				)));
		}
	}

	public class FakeMessageBrokerIdentifier : IMessageBrokerIdentifier
	{
		public Guid InstanceId { get; set; }
	}
}