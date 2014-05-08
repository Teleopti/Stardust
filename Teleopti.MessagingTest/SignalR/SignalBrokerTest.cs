using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
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

		private IHubProxy stubProxy()
		{
			var hubProxy = MockRepository.GenerateMock<IHubProxy>();
			hubProxy.Stub(x => x.Invoke("", null)).IgnoreArguments().Return(_doneTask);
			hubProxy.Stub(x => x.Subscribe("")).IgnoreArguments().Return(new Subscription());
			return hubProxy;
		}

		private signalBrokerForTest makeTarget(IHubProxy hubProxy)
		{
			var signalBroker = new signalBrokerForTest(new messageFilterManagerFake(), stubHubConnection(hubProxy),
													   new SignalSubscriber(hubProxy))
				{
					ConnectionString = @"http://localhost:8080"
				};
			signalBroker.StartMessageBroker();
			return signalBroker;
		}

		private signalBrokerForTest makeTarget(IHubProxy hubProxy, ISignalSubscriber signalSubscriber)
		{
			var signalBroker = new signalBrokerForTest(new messageFilterManagerFake(), stubHubConnection(hubProxy),
														  signalSubscriber)
			{
				ConnectionString = @"http://localhost:8080"
			};
			signalBroker.StartMessageBroker();
			return signalBroker;
		}

		private IHubConnectionWrapper stubHubConnection(IHubProxy hubProxy)
		{
			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.State).Return(ConnectionState.Connected);
			hubConnection.Stub(x => x.Start()).Return(makeDoneTask());
			hubConnection.Stub(x => x.CreateHubProxy("MessageBrokerHub")).Return(hubProxy);
			return hubConnection;
		}

		private static Task<object> makeDoneTask()
		{
			var taskCompletionSource = new TaskCompletionSource<object>();
			taskCompletionSource.SetResult(null);
			return taskCompletionSource.Task;
		}

		[Test]
		public void ShouldSendEventMessage()
		{
			var hubProxy = stubProxy();
			var target = makeTarget(hubProxy);
			
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

		[Test, Ignore("Unsubscribing isn't enabled, because it created bug 27055")]
		public void ShouldUnregisterEventSubscriptions()
		{
			var hubProxy = stubProxy();
			var target = makeTarget(hubProxy);
			
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
			var hubProxy = stubProxy();
			var signalSubscriber = MockRepository.GenerateMock<ISignalSubscriber>();
			var target = makeTarget(hubProxy, signalSubscriber);

			target.RegisterEventSubscription(string.Empty, Guid.Empty, (sender, args) => wasEventHandlerCalled = true,
			                                 typeof (IInterfaceForTest));
			target.SignalSubscriber.GetEventRaiser(x => x.OnNotification += null)
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
			var hubProxy = stubProxy();
			var signalSubscriber = MockRepository.GenerateMock<ISignalSubscriber>();
			var target = makeTarget(hubProxy, signalSubscriber);

			target.RegisterEventSubscription(string.Empty, Guid.Empty, (sender, args) => wasEventHandlerCalled = true,
			                                 typeof (IInterfaceForTest), DateTime.UtcNow.AddHours(-3),
			                                 DateTime.UtcNow.AddHours(-1));
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
		public void ShouldIgnoreInvokeForFutureSubscriptions()
		{
			var wasEventHandlerCalled = false;
			var hubProxy = stubProxy();
			var signalSubscriber = MockRepository.GenerateMock<ISignalSubscriber>();
			var target = makeTarget(hubProxy, signalSubscriber);

			target.RegisterEventSubscription(string.Empty, Guid.Empty, (sender, args) => wasEventHandlerCalled = true,
			                                 typeof (IInterfaceForTest), DateTime.UtcNow.AddHours(1),
			                                 DateTime.UtcNow.AddHours(3));

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
			public ISignalSubscriber SignalSubscriber { get; set; }

			public signalBrokerForTest(IMessageFilterManager typeFilter, IHubConnectionWrapper hubConnection, ISignalSubscriber signalSubscriber) : base(typeFilter)
			{
				_hubConnection = hubConnection;
				SignalSubscriber = signalSubscriber;
			}
			
			protected override ISignalSubscriber MakeSignalSubscriber(IHubProxy hubProxy)
			{
				return SignalSubscriber;
			}

			protected override IHubConnectionWrapper MakeHubConnection(Uri serverUrl)
			{
				return _hubConnection;
			}
		}
	}
}
