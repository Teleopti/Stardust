using System;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Events;
using Teleopti.Messaging.SignalR;
using Teleopti.Messaging.SignalR.Wrappers;

namespace Teleopti.MessagingTest.SignalR
{
	[TestFixture]
	public class SignalBrokerTest
	{
		
		private SignalBrokerForTest makeTarget(IHubProxyWrapper hubProxy)
		{
			var signalBroker = new SignalBrokerForTest(new MessageFilterManagerFake(), stubHubConnection(hubProxy));
			signalBroker.StartMessageBroker();
			return signalBroker;
		}

		private IHubConnectionWrapper stubHubConnection(IHubProxyWrapper hubProxy)
		{
			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.State).Return(ConnectionState.Connected);
			hubConnection.Stub(x => x.Start()).Return(TaskHelper.MakeDoneTask());
			hubConnection.Stub(x => x.CreateHubProxy("MessageBrokerHub")).Return(hubProxy);
			return hubConnection;
		}

		[Test]
		public void ShouldSendEventMessage()
		{
			var hubProxy = new HubProxyFake();
			var target = makeTarget(hubProxy);

			target.SendEventMessage(string.Empty, Guid.Empty, DateTime.UtcNow, DateTime.UtcNow, Guid.Empty, Guid.Empty,
									typeof(string), DomainUpdateType.Update, new byte[] { });

			hubProxy.NotifyClientsMultipleInvokedWith.Should().Have.Count.GreaterThan(0);
		}

		[Test]
		public void ShouldSendBatchEventMessages()
		{
			var hubProxy = new HubProxyFake();
			var target = makeTarget(hubProxy);

			target.SendEventMessages(string.Empty, Guid.Empty,
									 new IEventMessage[]
				                         {
					                         new EventMessage {DomainObjectType = "string"},
					                         new EventMessage {DomainObjectType = "string"}
				                         });

			hubProxy.NotifyClientsMultipleInvokedWith.Should().Have.Count.GreaterThan(0);
		}

		[Test]
		public void ShouldInvokeEventHandlers()
		{
			var wasEventHandlerCalled = false;
			var subscription = MockRepository.GenerateMock<ISubscriptionWrapper>();
			var hubProxy = new HubProxySubscribableFake(subscription);
			var target = makeTarget(hubProxy);

			target.RegisterEventSubscription(string.Empty, Guid.Empty, (sender, args) => wasEventHandlerCalled = true,
											 typeof(IInterfaceForTest));

			var notification = new Notification
										{
											DomainQualifiedType = "IInterfaceForTest",
											BusinessUnitId = Guid.Empty.ToString(),
											DataSource = string.Empty,
											DomainType = "IInterfaceForTest",
											StartDate = Subscription.DateToString(DateTime.UtcNow),
											EndDate = Subscription.DateToString(DateTime.UtcNow)
										};
			var token = JObject.Parse(JsonConvert.SerializeObject(notification));
			subscription.GetEventRaiser(x => x.Received += null).Raise(new List<JToken>(new JToken[] { token }));

			wasEventHandlerCalled.Should().Be(true);
		}

		[Test]
		public void ShouldIgnoreInvokeForOldSubscriptions()
		{
			var wasEventHandlerCalled = false;
			var subscription = MockRepository.GenerateMock<ISubscriptionWrapper>();
			var hubProxy = new HubProxySubscribableFake(subscription);
			var target = makeTarget(hubProxy);

			target.RegisterEventSubscription(string.Empty, Guid.Empty, (sender, args) => wasEventHandlerCalled = true,
											 typeof(IInterfaceForTest), DateTime.UtcNow.AddHours(-3),
											 DateTime.UtcNow.AddHours(-1));

			var notification = new Notification
			{
				DomainQualifiedType = "IInterfaceForTest",
				BusinessUnitId = Guid.Empty.ToString(),
				DataSource = string.Empty,
				DomainType = "IInterfaceForTest",
				StartDate = Subscription.DateToString(DateTime.UtcNow),
				EndDate = Subscription.DateToString(DateTime.UtcNow)
			};
			var token = JObject.Parse(JsonConvert.SerializeObject(notification));
			subscription.GetEventRaiser(x => x.Received += null).Raise(new List<JToken>(new JToken[] { token }));

			wasEventHandlerCalled.Should().Be(false);
		}

		[Test]
		public void ShouldIgnoreInvokeForFutureSubscriptions()
		{
			var wasEventHandlerCalled = false;
			var subscription = MockRepository.GenerateMock<ISubscriptionWrapper>();
			var hubProxy = new HubProxySubscribableFake(subscription);
			var target = makeTarget(hubProxy);

			target.RegisterEventSubscription(string.Empty, Guid.Empty, (sender, args) => wasEventHandlerCalled = true,
											 typeof(IInterfaceForTest), DateTime.UtcNow.AddHours(1),
											 DateTime.UtcNow.AddHours(3));

			var notification = new Notification
			{
				DomainQualifiedType = "IInterfaceForTest",
				BusinessUnitId = Guid.Empty.ToString(),
				DataSource = string.Empty,
				DomainType = "IInterfaceForTest",
				StartDate = Subscription.DateToString(DateTime.UtcNow),
				EndDate = Subscription.DateToString(DateTime.UtcNow)
			};
			var token = JObject.Parse(JsonConvert.SerializeObject(notification));
			subscription.GetEventRaiser(x => x.Received += null).Raise(new List<JToken>(new JToken[] { token }));

			wasEventHandlerCalled.Should().Be(false);
		}

		private interface IInterfaceForTest
		{

		}
	}
}
