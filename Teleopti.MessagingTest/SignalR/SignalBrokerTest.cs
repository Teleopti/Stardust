using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Events;
using Teleopti.Messaging.SignalR;
using Teleopti.Messaging.SignalR.Wrappers;
using Subscription = Microsoft.AspNet.SignalR.Client.Hubs.Subscription;

namespace Teleopti.MessagingTest.SignalR
{
	[TestFixture]
	public class SignalBrokerTest
	{
		private IHubProxyWrapper stubProxy()
		{
			return stubProxy(new SubscriptionWrapper(new Subscription()));
		}

		private IHubProxyWrapper stubProxy(ISubscriptionWrapper subscription)
		{
			var hubProxy = MockRepository.GenerateMock<IHubProxyWrapper>();
			hubProxy.Stub(x => x.Invoke("", null)).IgnoreArguments().Return(TaskHelper.MakeDoneTask());
			hubProxy.Stub(x => x.Subscribe("")).IgnoreArguments().Return(subscription);
			return hubProxy;
		}

		private signalBrokerForTest makeTarget(IHubProxyWrapper hubProxy)
		{
			var signalBroker = new signalBrokerForTest(new messageFilterManagerFake(), stubHubConnection(hubProxy))
				{
					ConnectionString = @"http://localhost:8080"
				};
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
			var hubProxy = stubProxy();
			var target = makeTarget(hubProxy);

			target.SendEventMessage(string.Empty, Guid.Empty, DateTime.UtcNow, DateTime.UtcNow, Guid.Empty, Guid.Empty,
									typeof(string), DomainUpdateType.Update, new byte[] { });

			hubProxy.AssertWasCalled(
				h =>
				h.Invoke(Arg<string>.Is.Equal("NotifyClientsMultiple"),
						 Arg<IEnumerable<Notification>>.Is.Anything));
		}

		[Test]
		public void ShouldSendBatchEventMessages()
		{
			var hubProxy = stubProxy();
			var target = makeTarget(hubProxy);

			target.SendEventMessages(string.Empty, Guid.Empty,
									 new IEventMessage[]
				                         {
					                         new EventMessage {DomainObjectType = "string"},
					                         new EventMessage {DomainObjectType = "string"}
				                         });

			hubProxy.AssertWasCalled(
				h =>
				h.Invoke(Arg<string>.Is.Equal("NotifyClientsMultiple"),
						 Arg<IEnumerable<Notification>>.Is.Anything));
		}

		[Test]
		public void ShouldRegisterEventSubscriptions()
		{
			var hubProxy = stubProxy();
			var target = makeTarget(hubProxy);

			target.RegisterEventSubscription(string.Empty, Guid.Empty, (sender, args) => { }, typeof(IInterfaceForTest));
			target.RegisterEventSubscription(string.Empty, Guid.Empty, (sender, args) => { }, Guid.Empty, typeof(string),
											 typeof(IInterfaceForTest));
			target.RegisterEventSubscription(string.Empty, Guid.Empty, (sender, args) => { }, typeof(IInterfaceForTest),
											 DateTime.UtcNow, DateTime.UtcNow);
			target.RegisterEventSubscription(string.Empty, Guid.Empty, (sender, args) => { }, Guid.Empty,
											 typeof(IInterfaceForTest), DateTime.UtcNow, DateTime.UtcNow);
			target.RegisterEventSubscription(string.Empty, Guid.Empty, (sender, args) => { }, Guid.Empty, typeof(string),
											 typeof(IInterfaceForTest), DateTime.UtcNow, DateTime.UtcNow);


			hubProxy.AssertWasCalled(
				h =>
				h.Invoke(Arg<string>.Is.Equal("AddSubscription"),
						 Arg<Subscription>.Is.Anything), a => a.Repeat.Times(5));
		}

		[Test, Ignore("Unsubscribing isn't enabled, because it created bug 27055")]
		public void ShouldUnregisterEventSubscriptions()
		{
			var hubProxy = stubProxy();
			var target = makeTarget(hubProxy);

			target.RegisterEventSubscription(string.Empty, Guid.Empty, EventMessageHandler, typeof(IInterfaceForTest));
			// TODO: UGLY, how to solve cleaner?
			Thread.Sleep(TimeSpan.FromMilliseconds(200));
			target.UnregisterEventSubscription(EventMessageHandler);

			hubProxy.AssertWasCalled(
				h =>
				h.Invoke(Arg<string>.Is.Equal("RemoveSubscription"),
						 Arg<string>.Is.Anything));
		}

		private static void EventMessageHandler(object sender, EventMessageArgs eventMessageArgs)
		{
		}

		[Test]
		public void ShouldInvokeEventHandlers()
		{
			var wasEventHandlerCalled = false;
			var subscription = MockRepository.GenerateMock<ISubscriptionWrapper>();
			var hubProxy = stubProxy(subscription);
			var target = makeTarget(hubProxy);

			target.RegisterEventSubscription(string.Empty, Guid.Empty, (sender, args) => wasEventHandlerCalled = true,
											 typeof(IInterfaceForTest));

			var notification = new Notification
										{
											DomainQualifiedType = "IInterfaceForTest",
											BusinessUnitId = Guid.Empty.ToString(),
											DataSource = string.Empty,
											DomainType = "IInterfaceForTest",
											StartDate = Interfaces.MessageBroker.Subscription.DateToString(DateTime.UtcNow),
											EndDate = Interfaces.MessageBroker.Subscription.DateToString(DateTime.UtcNow)
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
			var hubProxy = stubProxy(subscription);
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
				StartDate = Interfaces.MessageBroker.Subscription.DateToString(DateTime.UtcNow),
				EndDate = Interfaces.MessageBroker.Subscription.DateToString(DateTime.UtcNow)
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
			var hubProxy = stubProxy(subscription);
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
				StartDate = Interfaces.MessageBroker.Subscription.DateToString(DateTime.UtcNow),
				EndDate = Interfaces.MessageBroker.Subscription.DateToString(DateTime.UtcNow)
			};
			var token = JObject.Parse(JsonConvert.SerializeObject(notification));
			subscription.GetEventRaiser(x => x.Received += null).Raise(new List<JToken>(new JToken[] { token }));

			wasEventHandlerCalled.Should().Be(false);
		}

		private interface IInterfaceForTest
		{

		}

		private class messageFilterManagerFake : IMessageFilterManager
		{
			public bool HasType(Type type)
			{
				return true;
			}

			public string LookupTypeToSend(Type domainObjectType)
			{
				return "string";
			}

			public Type LookupType(Type domainObjectType)
			{
				return typeof(string);
			}
		}

		private class signalBrokerForTest : SignalBroker
		{
			private readonly IHubConnectionWrapper _hubConnection;

			public signalBrokerForTest(IMessageFilterManager typeFilter, IHubConnectionWrapper hubConnection)
				: base(typeFilter, new NoRecreate(), new Now())
			{
				_hubConnection = hubConnection;
			}

			protected override IHubConnectionWrapper MakeHubConnection(Uri serverUrl)
			{
				return _hubConnection;
			}
		}
	}

}
