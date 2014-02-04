using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client.Hubs;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Events;
using Teleopti.Messaging.SignalR;
using Subscription = Microsoft.AspNet.SignalR.Client.Hubs.Subscription;

namespace Teleopti.MessagingTest.SignalR
{
	[TestFixture]
	public class SignalBrokerTest
	{
		private Task _doneTask;

		[SetUp]
		public void Setup()
		{
			var taskCompletionSource = new TaskCompletionSource<object>();
			taskCompletionSource.SetResult(null);
			_doneTask = taskCompletionSource.Task;
		}

		[Test]
		public void ShouldSendEventMessage()
		{
			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.Start()).Return(_doneTask);
			var hubProxy = MockRepository.GenerateMock<IHubProxy>();
			hubProxy.Stub(x => x.Invoke("", null)).IgnoreArguments().Return(_doneTask);
			hubProxy.Stub(x => x.Subscribe("")).IgnoreArguments().Return(new Subscription());
			hubConnection.Stub(x => x.CreateHubProxy("MessageBrokerHub")).Return(hubProxy);
			var target = new signalBrokerForTest(new messageFilterManagerFake(), hubConnection, new SignalSubscriber(hubProxy))
				{
					ConnectionString = @"http://localhost:8080"
				};
			target.StartMessageBroker();
			target.SendEventMessage(string.Empty, Guid.Empty, DateTime.UtcNow, DateTime.UtcNow, Guid.Empty, Guid.Empty,
			                        typeof (string), DomainUpdateType.Update, new byte[] {});

			hubProxy.AssertWasCalled(
				h =>
				h.Invoke(Arg<string>.Is.Equal("NotifyClientsMultiple"),
				         Arg<IEnumerable<Notification>>.Is.Anything));
		}

		[Test]
		public void ShouldSendBatchEventMessages()
		{
			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.Start()).Return(_doneTask);
			var hubProxy = MockRepository.GenerateMock<IHubProxy>();
			hubProxy.Stub(x => x.Invoke("", null)).IgnoreArguments().Return(_doneTask);
			hubProxy.Stub(x => x.Subscribe("")).IgnoreArguments().Return(new Subscription());
			hubConnection.Stub(x => x.CreateHubProxy("MessageBrokerHub")).Return(hubProxy);
			var target = new signalBrokerForTest(new messageFilterManagerFake(), hubConnection, new SignalSubscriber(hubProxy))
				{
					ConnectionString = @"http://localhost:8080"
				};
			target.StartMessageBroker();
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
			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.Start()).Return(_doneTask);
			var hubProxy = MockRepository.GenerateMock<IHubProxy>();
			hubProxy.Stub(x => x.Invoke("", null)).IgnoreArguments().Return(_doneTask);
			hubProxy.Stub(x => x.Subscribe("")).IgnoreArguments().Return(new Subscription());
			hubConnection.Stub(x => x.CreateHubProxy("MessageBrokerHub")).Return(hubProxy);
			var target = new signalBrokerForTest(new messageFilterManagerFake(), hubConnection, new SignalSubscriber(hubProxy))
				{
					ConnectionString = @"http://localhost:8080"
				};
			target.StartMessageBroker();
			target.RegisterEventSubscription(string.Empty, Guid.Empty, (sender, args) => { }, typeof (IInterfaceForTest));
			target.RegisterEventSubscription(string.Empty, Guid.Empty, (sender, args) => { }, Guid.Empty, typeof (string),
			                                 typeof (IInterfaceForTest));
			target.RegisterEventSubscription(string.Empty, Guid.Empty, (sender, args) => { }, typeof (IInterfaceForTest),
			                                 DateTime.UtcNow, DateTime.UtcNow);
			target.RegisterEventSubscription(string.Empty, Guid.Empty, (sender, args) => { }, Guid.Empty,
			                                 typeof (IInterfaceForTest), DateTime.UtcNow, DateTime.UtcNow);
			target.RegisterEventSubscription(string.Empty, Guid.Empty, (sender, args) => { }, Guid.Empty, typeof (string),
			                                 typeof (IInterfaceForTest), DateTime.UtcNow, DateTime.UtcNow);


			hubProxy.AssertWasCalled(
				h =>
				h.Invoke(Arg<string>.Is.Equal("AddSubscription"),
				         Arg<Subscription>.Is.Anything), a => a.Repeat.Times(5));
		}

		[Test]
		public void ShouldUnregisterEventSubscriptions()
		{
			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.Start()).Return(_doneTask);
			var hubProxy = MockRepository.GenerateMock<IHubProxy>();
			hubProxy.Stub(x => x.Invoke("", null)).IgnoreArguments().Return(_doneTask);
			hubProxy.Stub(x => x.Subscribe("")).IgnoreArguments().Return(new Subscription());
			hubConnection.Stub(x => x.CreateHubProxy("MessageBrokerHub")).Return(hubProxy);
			var target = new signalBrokerForTest(new messageFilterManagerFake(), hubConnection, new SignalSubscriber(hubProxy))
				{
					ConnectionString = @"http://localhost:8080"
				};
			target.StartMessageBroker();
			target.RegisterEventSubscription(string.Empty, Guid.Empty, EventMessageHandler, typeof (IInterfaceForTest));
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
		public void ShouldCreateEventMessages()
		{
			var target = new signalBrokerForTest(null, null, null);
			var guid = Guid.NewGuid();
			var result = target.CreateEventMessage(guid, Guid.Empty, typeof (IInterfaceForTest), DomainUpdateType.Update);
			result.ModuleId.Should().Be(guid);

			result = target.CreateEventMessage(guid, Guid.Empty, typeof (IInterfaceForTest), Guid.Empty, typeof (string),
			                                   DomainUpdateType.Update);
			result.ModuleId.Should().Be(guid);

			result = target.CreateEventMessage(DateTime.UtcNow, DateTime.UtcNow, guid, Guid.Empty, typeof (IInterfaceForTest),
			                                   DomainUpdateType.Update);
			result.ModuleId.Should().Be(guid);

			result = target.CreateEventMessage(DateTime.UtcNow, DateTime.UtcNow, guid, Guid.Empty, typeof (string), Guid.Empty,
			                                   typeof (IInterfaceForTest), DomainUpdateType.Update);
			result.ModuleId.Should().Be(guid);

		}

		[Test]
		public void ShouldInvokeEventHandlers()
		{
			var wasEventHandlerCalled = false;

			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.Start()).Return(_doneTask);

			var hubProxy = MockRepository.GenerateMock<IHubProxy>();
			hubProxy.Stub(x => x.Invoke("", null)).IgnoreArguments().Return(_doneTask);

			hubProxy.Stub(x => x.Subscribe("")).IgnoreArguments().Return(new Subscription());
			hubConnection.Stub(x => x.CreateHubProxy("MessageBrokerHub")).Return(hubProxy);
			var signalSubscriber = MockRepository.GenerateMock<ISignalSubscriber>();
			var target = new signalBrokerForTest(new messageFilterManagerFake(), hubConnection, signalSubscriber)
				{
					ConnectionString = @"http://localhost:8080"
				};

			target.StartMessageBroker();
			target.RegisterEventSubscription(string.Empty, Guid.Empty, (sender, args) => wasEventHandlerCalled = true,
			                                 typeof (IInterfaceForTest));
			
			// TODO: How can we raise this without "cheating"
			signalSubscriber.GetEventRaiser(x => x.OnNotification += null)
			                .Raise(new object[]
				                {
					                new Notification
						                {
							                DomainQualifiedType = "IInterfaceForTest",
							                BusinessUnitId = Guid.Empty.ToString(),
							                DataSource = string.Empty,
							                DomainType = "IInterfaceForTest",
							                StartDate = Interfaces.MessageBroker.Subscription.DateToString(DateTime.UtcNow),
							                EndDate = Interfaces.MessageBroker.Subscription.DateToString(DateTime.UtcNow)
						                }
				                });

			wasEventHandlerCalled.Should().Be(true);
		}

		[Test]
		public void ShouldIgnoreInvokeForOldSubscriptions()
		{
			var wasEventHandlerCalled = false;

			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.Start()).Return(_doneTask);

			var hubProxy = MockRepository.GenerateMock<IHubProxy>();
			hubProxy.Stub(x => x.Invoke("", null)).IgnoreArguments().Return(_doneTask);

			hubProxy.Stub(x => x.Subscribe("")).IgnoreArguments().Return(new Subscription());
			hubConnection.Stub(x => x.CreateHubProxy("MessageBrokerHub")).Return(hubProxy);
			var signalSubscriber = MockRepository.GenerateMock<ISignalSubscriber>();
			var target = new signalBrokerForTest(new messageFilterManagerFake(), hubConnection, signalSubscriber)
			{
				ConnectionString = @"http://localhost:8080"
			};

			target.StartMessageBroker();
			target.RegisterEventSubscription(string.Empty, Guid.Empty, (sender, args) => wasEventHandlerCalled = true, typeof(IInterfaceForTest), DateTime.UtcNow.AddHours(-3), DateTime.UtcNow.AddHours(-1));

			// TODO: How can we raise this without "cheating"
			signalSubscriber.GetEventRaiser(x => x.OnNotification += null)
							.Raise(new object[]
				                {
					                new Notification
						                {
							                DomainQualifiedType = "IInterfaceForTest",
							                BusinessUnitId = Guid.Empty.ToString(),
							                DataSource = string.Empty,
							                DomainType = "IInterfaceForTest",
							                StartDate = Interfaces.MessageBroker.Subscription.DateToString(DateTime.UtcNow),
							                EndDate = Interfaces.MessageBroker.Subscription.DateToString(DateTime.UtcNow)
						                }
				                });

			wasEventHandlerCalled.Should().Be(false);
		}

		[Test]
		// TODO: Ehhh future?
		public void ShouldIgnoreInvokeForFutureSubscriptions()
		{
			var wasEventHandlerCalled = false;

			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.Start()).Return(_doneTask);

			var hubProxy = MockRepository.GenerateMock<IHubProxy>();
			hubProxy.Stub(x => x.Invoke("", null)).IgnoreArguments().Return(_doneTask);

			hubProxy.Stub(x => x.Subscribe("")).IgnoreArguments().Return(new Subscription());
			hubConnection.Stub(x => x.CreateHubProxy("MessageBrokerHub")).Return(hubProxy);
			var signalSubscriber = MockRepository.GenerateMock<ISignalSubscriber>();
			var target = new signalBrokerForTest(new messageFilterManagerFake(), hubConnection, signalSubscriber)
			{
				ConnectionString = @"http://localhost:8080"
			};

			target.StartMessageBroker();
			target.RegisterEventSubscription(string.Empty, Guid.Empty, (sender, args) => wasEventHandlerCalled = true,
			                                 typeof (IInterfaceForTest), DateTime.UtcNow.AddHours(1),
			                                 DateTime.UtcNow.AddHours(3));

			// TODO: How can we raise this without "cheating"
			signalSubscriber.GetEventRaiser(x => x.OnNotification += null)
							.Raise(new object[]
				                {
					                new Notification
						                {
							                DomainQualifiedType = "IInterfaceForTest",
							                BusinessUnitId = Guid.Empty.ToString(),
							                DataSource = string.Empty,
							                DomainType = "IInterfaceForTest",
							                StartDate = Interfaces.MessageBroker.Subscription.DateToString(DateTime.UtcNow),
							                EndDate = Interfaces.MessageBroker.Subscription.DateToString(DateTime.UtcNow)
						                }
				                });

			wasEventHandlerCalled.Should().Be(false);
		}

		[Test]
		public void Really_ShouldStartBroker()
		{
			Assert.Ignore("Test describing existing functionality");
		}

		[Test]
		public void Really_ShouldStopBroker()
		{
			Assert.Ignore("Test describing existing functionality");
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
				return typeof (string);
			}
		}

		private class signalBrokerForTest : SignalBroker
		{
			private readonly IHubConnectionWrapper _hubConnection;
			private readonly ISignalSubscriber _signalSubscriber;

			public signalBrokerForTest(IMessageFilterManager typeFilter, IHubConnectionWrapper hubConnection, ISignalSubscriber signalSubscriber) : base(typeFilter)
			{
				_hubConnection = hubConnection;
				_signalSubscriber = signalSubscriber;
			}

			protected override ISignalSubscriber MakeSignalSubscriber(IHubProxy hubProxy)
			{
				return _signalSubscriber;
			}

			protected override IHubConnectionWrapper MakeHubConnection(Uri serverUrl)
			{
				return _hubConnection;
			}
		}
	}
}
