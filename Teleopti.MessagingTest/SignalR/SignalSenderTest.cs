using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client.Hubs;
using NUnit.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Messaging.SignalR;
using log4net;
using Subscription = Microsoft.AspNet.SignalR.Client.Hubs.Subscription;

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
			return new signalSenderForTest(hubConnection, new Now());
		}

		private signalSenderForTest makeSignalSender(IHubProxy hubProxy)
		{
			return makeSignalSender(hubProxy, new Now());
		}

		private signalSenderForTest makeSignalSender(IHubProxy hubProxy, ILog logger)
		{
			return makeSignalSender(hubProxy, new Now(), logger);
		}

		private signalSenderForTest makeSignalSender(IHubProxy hubProxy, INow now)
		{
			return makeSignalSender(hubProxy, now, null);
		}

		private signalSenderForTest makeSignalSender(IHubProxy hubProxy, INow now, ILog logger)
		{
			var signalSender = new signalSenderForTest(stubHubConnection(hubProxy), now, logger);
			signalSender.InstantiateBrokerService();
			return signalSender;
		}

		private hubProxyFake stubProxy()
		{
			return new hubProxyFake();
		}

		private IHubConnectionWrapper stubHubConnection(IHubProxy hubProxy)
		{
			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.Start()).Return(makeDoneTask());
			hubConnection.Stub(x => x.CreateHubProxy("MessageBrokerHub")).Return(hubProxy);
			return hubConnection;
		}

		[Test]
		public void ShouldBatchNotifications()
		{
			var hubProxy = stubProxy();
			var target = makeSignalSender(hubProxy);

			var notification1 = new Notification();
			var notification2 = new Notification();

			target.SendNotificationAsync(notification1);
			target.SendNotificationAsync(notification2);
			target.ProcessTheQueue();

			hubProxy.NotifyClientsMultipleInvokedWith.First().Should().Have.SameValuesAs(new[] {notification1, notification2});
		}

		[Test]
		public void ShouldBatchTwentyNotificationsAtATime()
		{
			var hubProxy = stubProxy();
			var target = makeSignalSender(hubProxy);

			var notifications1 = Enumerable.Range(1, 20).Select(i => new Notification()).ToArray();
			var notifications2 = Enumerable.Range(1, 10).Select(i => new Notification()).ToArray();
			notifications1.ForEach(target.SendNotificationAsync);
			notifications2.ForEach(target.SendNotificationAsync);

			target.ProcessTheQueue();
			target.ProcessTheQueue();

			hubProxy.NotifyClientsMultipleInvokedWith[0].Should().Have.SameValuesAs(notifications1);
			hubProxy.NotifyClientsMultipleInvokedWith[1].Should().Have.SameValuesAs(notifications2);
		}

		[Test]
		public void ShouldDiscardBatchNotificationsOlderThanTwoMinutes()
		{
			var now = new MutableNow();
			var hubProxy = stubProxy();
			var target = makeSignalSender(hubProxy, now);

			now.Mutate(DateTime.UtcNow.AddMinutes(-2));
			var oldNotification = new Notification();
			target.SendNotificationAsync(oldNotification);
			now.Mutate(DateTime.UtcNow);
			var newNotification = new Notification();
			target.SendNotificationAsync(newNotification);
			target.ProcessTheQueue();

			hubProxy.NotifyClientsMultipleInvokedWith.Single().Should().Have.SameValuesAs(new[] {newNotification});
		}

		[Test]
		public void ShouldLogAndIgnoreOnExceptionInvokingProxy()
		{
			var hubProxy = MockRepository.GenerateMock<IHubProxy>();
			var log = MockRepository.GenerateMock<ILog>();
			var target = makeSignalSender(hubProxy, log);

			hubProxy.Stub(x => x.Invoke("NotifyClientsMultiple", null)).IgnoreArguments().Throw(new InvalidOperationException());

			Assert.DoesNotThrow(() => target.SendNotificationAsync(new Notification()));
			target.ProcessTheQueue();
			log.AssertWasCalled(t => t.Error("", null), a => a.IgnoreArguments());
		}

		[Test]
		public void ShouldLogAndIgnoreOnExceptionSendingNotification()
		{
			var failedTask = makeFailedTask(new Exception());
			var hubProxy = MockRepository.GenerateMock<IHubProxy>();
			var log = MockRepository.GenerateMock<ILog>();
			var target = makeSignalSender(hubProxy, log);

			hubProxy.Stub(x => x.Invoke("NotifyClientsMultiple", null)).IgnoreArguments().Return(failedTask);

			Assert.DoesNotThrow(() => target.SendNotificationAsync(new Notification()));
			target.ProcessTheQueue();
			log.AssertWasCalled(t => t.Error("",null), a => a.IgnoreArguments());
		}
		
		[Test]
		public void ShouldRestartHubConnectionWhenConnectionClosed()
		{
			var hubProxy = stubProxy();
			var hubConnection = stubHubConnection(hubProxy);
			var target = makeSignalSender(hubConnection);
			target.InstantiateBrokerService();

			hubConnection.GetEventRaiser(x => x.Closed += null).Raise();

			hubConnection.AssertWasCalled(x => x.Start(), a => a.Repeat.Twice());
		}

		[Test]
		public void ShouldRstartHubConnectionWhenStartFails()
		{
			var hubProxy = stubProxy();
			var hubConnection = stubHubConnection(hubProxy);
			var target = makeSignalSender(hubConnection);
			target.InstantiateBrokerService();

			hubConnection.Stub(x => x.Start()).Return(makeFailedTask(new Exception())).Repeat.Once();

			hubConnection.AssertWasCalled(x => x.Start());
		}

		private class hubProxyFake : IHubProxy
		{
			public readonly IList<IEnumerable<Notification>> NotifyClientsMultipleInvokedWith = new List<IEnumerable<Notification>>();

			public Task Invoke(string method, params object[] args)
			{
				if (method == "NotifyClientsMultiple")
					NotifyClientsMultipleInvokedWith.Add(args.First() as IEnumerable<Notification>);
				return makeDoneTask();
			}

			public Task<T> Invoke<T>(string method, params object[] args)
			{
				throw new NotImplementedException();
			}

			public Subscription Subscribe(string eventName)
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
			private readonly INow _now;

			public signalSenderForTest(IHubConnectionWrapper hubConnection, INow now, ILog logger) : this(hubConnection, now)
			{
				_logger = logger;
			}

			public signalSenderForTest(IHubConnectionWrapper hubConnection, INow now)
				: base(null)
			{
				_hubConnection = hubConnection;
				_now = now;
			}

			protected override ILog MakeLogger()
			{
				return _logger ?? base.MakeLogger();
			}

			protected override IHubConnectionWrapper MakeHubConnection()
			{
				return _hubConnection;
			}

			protected override DateTime CurrentUtcTime()
			{
				return _now.UtcDateTime();
			}

			protected override void StartWorkerThread()
			{
			}

			public void ProcessTheQueue()
			{
				base.ProcessTheQueue();
			}

		}
	}
}
