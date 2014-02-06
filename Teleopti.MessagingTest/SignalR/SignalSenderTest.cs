using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client.Hubs;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Messaging.Exceptions;
using Teleopti.Messaging.SignalR;
using log4net;

namespace Teleopti.MessagingTest.SignalR
{
	[TestFixture, Ignore]
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

		private signalSenderForTest makeNotStartedSignalSender(IHubConnectionWrapper hubConnection)
		{
			var hubProxy = stubProxy();
			hubConnection.Stub(x => x.CreateHubProxy("MessageBrokerHub")).Return(hubProxy);
			return new signalSenderForTest(hubConnection, new Now());
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

		private IHubProxy stubProxy()
		{
			var hubProxy = MockRepository.GenerateMock<IHubProxy>();
			hubProxy.Stub(x => x.Invoke("", null)).IgnoreArguments().Return(makeDoneTask());
			return hubProxy;
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
			target.WaitUntilAllNotificationsAreSent();

			hubProxy.AssertWasCalled(
				h =>
				h.Invoke(Arg<string>.Is.Equal("NotifyClientsMultiple"),
				         Arg<IEnumerable<Notification>>.List.ContainsAll(new[] {notification1, notification2})));
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
			target.WaitUntilAllNotificationsAreSent();

			hubProxy.AssertWasCalled(h => h.Invoke("NotifyClientsMultiple", new object[]{notifications1}));
			hubProxy.AssertWasCalled(h => h.Invoke("NotifyClientsMultiple", new object[]{notifications2}));
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
			target.WaitUntilAllNotificationsAreSent();

			hubProxy.AssertWasNotCalled(h => h.Invoke("NotifyClientsMultiple", new object[] { oldNotification }));
			hubProxy.AssertWasCalled(h => h.Invoke("NotifyClientsMultiple", new object[] {newNotification}));
		}

		[Test]
		public void ShouldLogAndIgnoreOnExceptionInvokingProxy()
		{
			var hubProxy = stubProxy();
			var log = MockRepository.GenerateMock<ILog>();
			var target = makeSignalSender(hubProxy, log);

			hubProxy.Stub(x => x.Invoke("NotifyClientsMultiple", new Notification())).Throw(new Exception());

			Assert.DoesNotThrow(() => target.SendNotificationAsync(new Notification()));
			log.AssertWasCalled(t => t.ErrorFormat(""), a => a.IgnoreArguments());
		}

		[Test]
		public void ShouldLogAndIgnoreOnExceptionSendingNotification()
		{
			var failedTask = makeFailedTask(new Exception());
			var hubProxy = stubProxy();
			var log = MockRepository.GenerateMock<ILog>();
			var target = makeSignalSender(hubProxy, log);

			hubProxy.Stub(x => x.Invoke("NotifyClientsMultiple", new Notification())).Return(failedTask);

			Assert.DoesNotThrow(() => target.SendNotificationAsync(new Notification()));
			log.AssertWasCalled(t => t.ErrorFormat(""), a => a.IgnoreArguments());
		}
		
		[Test]
		public void ShouldRestartHubConnectionWhenConnectionClosed()
		{
			var hubProxy = stubProxy();
			var hubConnection = stubHubConnection(hubProxy);
			var target = makeSignalSender(hubConnection);

			hubConnection.Raise(x => x.Closed += null, hubConnection, EventArgs.Empty);

			hubConnection.AssertWasCalled(x => x.Start(), a => a.Repeat.Twice());
		}

		

		[Test]
		public void ShouldThrowBrokerNotInstanciatedExceptionOnAggregateExceptionWhenStarting()
		{
			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.Start()).Return(makeFailedTask(new AggregateException()));
			var target = makeNotStartedSignalSender(hubConnection);

			Assert.Throws<BrokerNotInstantiatedException>(target.InstantiateBrokerService);
		}

		[Test]
		public void ShouldThrowBrokerNotInstanciatedExceptionOnAggregateExceptionWhenInvokingStart()
		{
			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.Start()).Throw(new AggregateException());
			var target = makeNotStartedSignalSender(hubConnection);

			Assert.Throws<BrokerNotInstantiatedException>(target.InstantiateBrokerService);
		}

		[Test]
		public void ShouldThrowBrokerNotInstanciatedExceptionOnSocketExceptionWhenStarting()
		{
			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.Start()).Return(makeFailedTask(new SocketException()));
			var target = makeNotStartedSignalSender(hubConnection);

			Assert.Throws<BrokerNotInstantiatedException>(target.InstantiateBrokerService);
		}

		[Test]
		public void ShouldThrowBrokerNotInstanciatedExceptionOnSocketExceptionWhenInvokingStart()
		{
			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.Start()).Throw(new SocketException());
			var target = makeNotStartedSignalSender(hubConnection);

			Assert.Throws<BrokerNotInstantiatedException>(target.InstantiateBrokerService);
		}

		[Test]
		public void ShouldThrowBrokerNotInstanciatedExceptionOnInvalidOperationExceptionWhenStarting()
		{
			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.Start()).Return(makeFailedTask(new InvalidOperationException()));
			var target = makeNotStartedSignalSender(hubConnection);

			Assert.Throws<BrokerNotInstantiatedException>(target.InstantiateBrokerService);
		}

		[Test]
		public void ShouldThrowBrokerNotInstanciatedExceptionOnInvalidOperationExceptionWhenInvokingStart()
		{
			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.Start()).Throw(new InvalidOperationException());
			var target = makeNotStartedSignalSender(hubConnection);

			Assert.Throws<BrokerNotInstantiatedException>(target.InstantiateBrokerService);
		}

		private class signalSenderForTest : SignalSender
		{
			private readonly IHubConnectionWrapper _hubConnection;
			private readonly INow _now;
			private readonly ILog _logger;

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

		}
	}
}
