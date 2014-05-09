using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using NUnit.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Messaging.SignalR;
using log4net;
using Teleopti.Messaging.SignalR.Wrappers;

namespace Teleopti.MessagingTest.SignalR
{
	[TestFixture]
	public class SignalSenderTest
	{
		private static Task<object> makeFailedTask(Exception ex)
		{
			var taskCompletionSource = new TaskCompletionSource<object>();
			taskCompletionSource.SetException(ex);			
			return taskCompletionSource.Task;
		}

		private static Task<object> makeDoneTask()
		{
			var taskCompletionSource = new TaskCompletionSource<object>();
			taskCompletionSource.SetResult(null);
			return taskCompletionSource.Task;
		}

		private signalSenderForTest makeSignalSender(IHubConnectionWrapper hubConnection)
		{
			return new signalSenderForTest(hubConnection, null);
		}

		private signalSenderForTest makeSignalSender(IHubConnectionWrapper hubConnection, ILog log)
		{
			return new signalSenderForTest(hubConnection, log);
		}

		private signalSenderForTest makeSignalSender(IHubProxyWrapper hubProxy, ILog logger = null)
		{
			var signalSender = new signalSenderForTest(stubHubConnection(hubProxy), logger);
			signalSender.StartBrokerService();
			return signalSender;
		}

		private SignalSender makeSingleSignalSender(hubProxyFake proxy)
		{
			var signalSingleSender = new signalSenderForTest(stubHubConnection(proxy), null);
			signalSingleSender.StartBrokerService();
			return signalSingleSender;
		}

		private hubProxyFake stubProxy()
		{
			return new hubProxyFake();
		}

		private IHubConnectionWrapper stubHubConnection(IHubProxyWrapper hubProxy)
		{
			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.State).Return(ConnectionState.Connected);
			hubConnection.Stub(x => x.Start()).Return(makeDoneTask());
			hubConnection.Stub(x => x.CreateHubProxy("MessageBrokerHub")).Return(hubProxy);
			return hubConnection;
		}
		
		[Test]
		public void ShouldSendSingleNotification()
		{
			var hubProxy = stubProxy();
			var target = makeSingleSignalSender(hubProxy);

			var notification1 = new Notification();

			target.SendNotification(notification1);

			hubProxy.NotifyClientsInvokedWith.First().Should().Be(notification1);
		}

		[Test]
		public void ShouldRestartHubConnectionWhenConnectionClosed()
		{
			var hubProxy = stubProxy();
			var hubConnection = stubHubConnection(hubProxy);
			var target = makeSignalSender(hubConnection);
			target.StartBrokerService(TimeSpan.FromSeconds(0));

			hubConnection.GetEventRaiser(x => x.Closed += null).Raise();

			hubConnection.AssertWasCalled(x => x.Start(), a => a.Repeat.Twice());
		}

		[Test]
		public void ShouldNotRestartHubConnectionMoreThanXTimes()
		{
			const int x = 3;
			var hubproxy = stubProxy();
			var hubConnection = stubHubConnection(hubproxy);
			var target = makeSignalSender(hubConnection);
			target.StartBrokerService(TimeSpan.FromSeconds(0), x);

			hubConnection.GetEventRaiser(c => c.Closed += null).Raise();
			hubConnection.GetEventRaiser(c => c.Closed += null).Raise();
			hubConnection.GetEventRaiser(c => c.Closed += null).Raise();
			hubConnection.GetEventRaiser(c => c.Closed += null).Raise();

			hubConnection.AssertWasCalled(c => c.Start(), a => a.Repeat.Times(4));
		}

		[Test]
		public void ShouldContinueToRestartWhenReconnectAttemptsIsNotSet()
		{
			var hubproxy = stubProxy();
			var hubConnection = stubHubConnection(hubproxy);
			var target = makeSignalSender(hubConnection);
			target.StartBrokerService(TimeSpan.FromSeconds(0));

			hubConnection.GetEventRaiser(c => c.Closed += null).Raise();
			hubConnection.GetEventRaiser(c => c.Closed += null).Raise();
			hubConnection.GetEventRaiser(c => c.Closed += null).Raise();
			hubConnection.GetEventRaiser(c => c.Closed += null).Raise();
			hubConnection.GetEventRaiser(c => c.Closed += null).Raise();

			hubConnection.AssertWasCalled(c => c.Start(), a => a.Repeat.Times(6));
		}


		[Test]
		public void ShouldRestartHubConnectionWhenStartFails()
		{
			var hubProxy = stubProxy();
			var hubConnection = stubHubConnection(hubProxy);
			var target = makeSignalSender(hubConnection);
			target.StartBrokerService(TimeSpan.FromSeconds(0));

			hubConnection.Stub(x => x.Start()).Return(makeFailedTask(new Exception())).Repeat.Once();
			
			hubConnection.AssertWasCalled(x => x.Start());
		}

		[Test]
		public void ShouldLogWhenNotificationTasksFails()
		{
			var failedTask = makeFailedTask(new Exception());
			var hubProxy = MockRepository.GenerateMock<IHubProxyWrapper>();

			var log = MockRepository.GenerateMock<ILog>();
			var target = makeSignalSender(hubProxy, log);

			hubProxy.Stub(x => x.Invoke("NotifyClientsMultiple", null)).IgnoreArguments().Return(failedTask);

			Assert.DoesNotThrow(() => target.SendNotification(new Notification()));
			log.AssertWasCalled(t => t.Debug("", null), a => a.IgnoreArguments());
		}

		[Test]
		public void ShouldLogWhenTryingToReconnect()
		{
			var hubProxy = stubProxy();
			var hubConnection = stubHubConnection(hubProxy);
			var log = MockRepository.GenerateMock<ILog>();

			var target = makeSignalSender(hubConnection, log);
			target.StartBrokerService(TimeSpan.FromSeconds(0));

			hubConnection.GetEventRaiser(x => x.Closed += null).Raise();

			log.AssertWasCalled(x => x.Error(SignalConnection.ConnectionRestartedErrorMessage));
		}

		[Test]
		public void ShouldLogWhenReconnected()
		{
			var hubProxy = stubProxy();
			var hubConnection = stubHubConnection(hubProxy);
			var log = MockRepository.GenerateMock<ILog>();

			var target = makeSignalSender(hubConnection, log);
			target.StartBrokerService(TimeSpan.FromSeconds(0));

			hubConnection.GetEventRaiser(x => x.Reconnected += null).Raise();

			log.AssertWasCalled(x => x.Info(SignalConnection.ConnectionReconnected));
			
		}

		private class hubProxyFake : IHubProxyWrapper
		{
			public readonly IList<Notification> NotifyClientsInvokedWith = new List<Notification>();

			public Task Invoke(string method, params object[] args)
			{
				if (method == "NotifyClients")
					NotifyClientsInvokedWith.Add(args.First() as Notification);
				return makeDoneTask();
			}

			public Task<T> Invoke<T>(string method, params object[] args)
			{
				throw new NotImplementedException();
			}

			public ISubscriptionWrapper Subscribe(string eventName)
			{
				throw new NotImplementedException();
			}

			public JToken this[string name]
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			public JsonSerializer JsonSerializer { get; private set; }
		}


		private class signalSenderForTest : SignalSender
		{
			private readonly IHubConnectionWrapper _hubConnection;

			public signalSenderForTest(IHubConnectionWrapper hubConnection, ILog logger)
				: base("http://neeedsToBeSet")
			{
				_hubConnection = hubConnection;
				Logger = logger;
			}

			protected override ILog MakeLogger()
			{
				return Logger ?? base.MakeLogger();
			}

			protected override IHubConnectionWrapper MakeHubConnection()
			{
				return _hubConnection;
			}
		}	
	}
}
