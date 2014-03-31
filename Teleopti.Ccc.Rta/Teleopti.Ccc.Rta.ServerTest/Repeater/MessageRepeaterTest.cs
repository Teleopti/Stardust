using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Rta.Server.Repeater;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.Rta.ServerTest.Repeater
{
	public class MessageRepeaterTest
	{
		[Test]
		public void ShouldRepeatMessageWhenTriggered()
		{
			var messageSender = MockRepository.GenerateMock<IMessageSender>();
			var createNotification = MockRepository.GenerateMock<ICreateNotification>();
			var agentState = new ActualAgentState();
			var notification = new Notification();
			var trigger = new SimpleTrigger();
			createNotification.Expect(c => c.FromActualAgentState(agentState)).Return(notification);
			var target = new MessageRepeater(messageSender, trigger, createNotification);

			target.Invoke(agentState);
			trigger.TriggerAction();

			messageSender.AssertWasCalled(x => x.SendNotification(notification));
		}

		[Test]
		public void ShouldNotRepeatIfNoTrigger()
		{
			var messageSender = MockRepository.GenerateMock<IMessageSender>();
			var createNotification = MockRepository.GenerateMock<ICreateNotification>();
			var agentState = new ActualAgentState();
			var notification = new Notification();
			createNotification.Expect(c => c.FromActualAgentState(agentState)).Return(notification);
			var target = new MessageRepeater(messageSender, null, createNotification);

			target.Invoke(agentState);

			messageSender.AssertWasNotCalled(x => x.SendNotification(notification));
		}

		[Test]
		public void ShouldRepeatOneMessagePerPerson()
		{
			var messageSender = MockRepository.GenerateMock<IMessageSender>();
			var createNotification = MockRepository.GenerateMock<ICreateNotification>();
			var agentState = new ActualAgentState{PersonId = Guid.NewGuid()};
			var agentState2 = new ActualAgentState{PersonId = Guid.NewGuid()};
			var notification = new Notification();
			var notification2 = new Notification();
			var trigger = new SimpleTrigger();
			createNotification.Stub(c => c.FromActualAgentState(agentState)).Return(notification);
			createNotification.Stub(c => c.FromActualAgentState(agentState2)).Return(notification2);
			var target = new MessageRepeater(messageSender, trigger, createNotification);

			target.Invoke(agentState);
			target.Invoke(agentState2);
			trigger.TriggerAction();

			messageSender.AssertWasCalled(x => x.SendNotification(notification));
			messageSender.AssertWasCalled(x => x.SendNotification(notification2));
		}

		[Test]
		public void Flush_WhenMultipleStatesForOnePerson_ShouldOnlyGenerateOneMessage()
		{
			var messageSender = MockRepository.GenerateMock<IMessageSender>();
			var createNotification = MockRepository.GenerateMock<ICreateNotification>();
			var agentState = new ActualAgentState { PersonId = Guid.NewGuid() };
			var notification = new Notification();
			var trigger = new SimpleTrigger();
			createNotification.Stub(c => c.FromActualAgentState(agentState)).Return(notification);
			var target = new MessageRepeater(messageSender, trigger, createNotification);

			target.Invoke(agentState);
			target.Invoke(agentState);
			trigger.TriggerAction();

			messageSender.AssertWasCalled(x => x.SendNotification(notification), (a) => a.Repeat.Once());
		}

		[Test]
		public void Flush_WhenAlreadyFlushedWithoutAnyNewStateChanges_ShouldNotSendAnyNewMessages()
		{
			var messageSender = MockRepository.GenerateMock<IMessageSender>();
			var createNotification = MockRepository.GenerateMock<ICreateNotification>();
			var agentState = new ActualAgentState { PersonId = Guid.NewGuid() };
			var notification = new Notification();
			var trigger = new SimpleTrigger();
			createNotification.Stub(c => c.FromActualAgentState(agentState)).Return(notification);
			var target = new MessageRepeater(messageSender, trigger, createNotification);

			target.Invoke(agentState);
			trigger.TriggerAction();
			trigger.TriggerAction();

			messageSender.AssertWasCalled(x => x.SendNotification(notification), a => a.Repeat.Once());
		}
	}
}