﻿using System;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Client.SignalR;
using Teleopti.Messaging.Client.SignalR.Wrappers;
using Teleopti.MessagingTest.SignalR.ImplementationDetailsTests.TestDoubles;

namespace Teleopti.MessagingTest.SignalR.ImplementationDetailsTests
{
	[TestFixture]
	public class SignalRSubscriptionsTest
	{
		private MessageBrokerCompositeClientForTest makeTarget(IHubProxyWrapper hubProxy)
		{
			var signalBroker = MessageBrokerCompositeClientForTest.Make(new MessageFilterManagerFake(), stubHubConnection(hubProxy));
			signalBroker.StartBrokerService();
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
		public void ShouldInvokeSubscriptionCallback()
		{
			var wasEventHandlerCalled = false;
			var subscription = MockRepository.GenerateMock<ISubscriptionWrapper>();
			var hubProxy = new HubProxySubscribableFake(subscription);
			var target = makeTarget(hubProxy);

			target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => wasEventHandlerCalled = true,
				typeof(IInterfaceForTest));

			var notification = new Message
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
		public void ShouldNotThrowWhenSubscribingWhenNotStarted()
		{
			var subscription = MockRepository.GenerateMock<ISubscriptionWrapper>();
			var hubProxy = new HubProxySubscribableFake(subscription);
			var target = MessageBrokerCompositeClientForTest.Make(new MessageFilterManagerFake(), stubHubConnection(hubProxy));

			Assert.DoesNotThrow(() => target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => {}, typeof(IInterfaceForTest)));
		}

		[Test]
		public void ShouldNotThrowWhenUnsubscribingWhenNotStarted()
		{
			var subscription = MockRepository.GenerateMock<ISubscriptionWrapper>();
			var hubProxy = new HubProxySubscribableFake(subscription);
			var target = MessageBrokerCompositeClientForTest.Make(new MessageFilterManagerFake(), stubHubConnection(hubProxy));

			Assert.DoesNotThrow(() => target.UnregisterSubscription((sender, args) => {}));
		}

		[Test]
		public void ShouldIgnoreInvokeForOldSubscriptions()
		{
			var wasEventHandlerCalled = false;
			var subscription = MockRepository.GenerateMock<ISubscriptionWrapper>();
			var hubProxy = new HubProxySubscribableFake(subscription);
			var target = makeTarget(hubProxy);

			target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => wasEventHandlerCalled = true,
				typeof(IInterfaceForTest), DateTime.UtcNow.AddHours(-3),
				DateTime.UtcNow.AddHours(-1));

			var notification = new Message
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

			target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => wasEventHandlerCalled = true,
				typeof(IInterfaceForTest), DateTime.UtcNow.AddHours(1),
				DateTime.UtcNow.AddHours(3));

			var notification = new Message
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
		public void ShouldSubscribeToNotificationsOnFirstConnection()
		{
			var time = new FakeTime();
			var wasEventHandlerCalled = false;
			var hubProxy1 = new HubProxyFake();
			var hubConnection1 = stubHubConnection(hubProxy1);
			var target = MultiConnectionMessageBrokerCompositeClientForTest.Make(new MessageFilterManagerFake(),
				new[] { hubConnection1 }, new RecreateOnNoPingReply(TimeSpan.FromMinutes(1)), time);
			target.StartBrokerService();

			target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => wasEventHandlerCalled = true,
				typeof(IInterfaceForTest));
			target.Send(string.Empty, Guid.Empty, DateTime.UtcNow, DateTime.UtcNow, Guid.Empty, Guid.Empty,
				typeof(IInterfaceForTest), DomainUpdateType.Update, new byte[] { });

			wasEventHandlerCalled.Should().Be(true);
		}

		[Test]
		public void ShouldSubscribeToNotificationsOnRecreatedConnection()
		{
			var time = new FakeTime();
			var wasEventHandlerCalled = false;
			var hubProxy1 = new HubProxyFake();
			var hubProxy2 = new HubProxyFake();
			var hubConnection1 = stubHubConnection(hubProxy1);
			var hubConnection2 = stubHubConnection(hubProxy2);
			var target = MultiConnectionMessageBrokerCompositeClientForTest.Make(new MessageFilterManagerFake(),
				new[] { hubConnection1, hubConnection2 }, new RecreateOnNoPingReply(TimeSpan.FromMinutes(1)), time);
			target.StartBrokerService();

			target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => wasEventHandlerCalled = true,
				typeof(IInterfaceForTest));
			hubProxy1.BreakTheConnection();
			time.Passes(TimeSpan.FromMinutes(2));
			target.Send(string.Empty, Guid.Empty, DateTime.UtcNow, DateTime.UtcNow, Guid.Empty, Guid.Empty,
				typeof(IInterfaceForTest), DomainUpdateType.Update, new byte[] { });

			wasEventHandlerCalled.Should().Be(true);
		}

		private interface IInterfaceForTest
		{

		}
	}
}