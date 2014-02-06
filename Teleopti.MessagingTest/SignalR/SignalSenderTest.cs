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
	[TestFixture]
	public class SignalSenderTest
	{
		private Task _doneTask;

		[SetUp]
		public void Setup()
		{
			var taskCompletionSource = new TaskCompletionSource<object>();
			taskCompletionSource.SetResult(null);
			_doneTask = taskCompletionSource.Task;
		}

		private Tuple<IHubProxy, signalSenderForTest, IHubConnectionWrapper> makeNotStartedSignalSender()
		{
			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			var hubProxy = MockRepository.GenerateMock<IHubProxy>();
			hubProxy.Stub(x => x.Invoke("", null)).IgnoreArguments().Return(_doneTask);
			hubConnection.Stub(x => x.CreateHubProxy("MessageBrokerHub")).Return(hubProxy);
			var target = new signalSenderForTest(hubConnection, new Now());
			//target.InstantiateBrokerService();
			return new Tuple<IHubProxy, signalSenderForTest, IHubConnectionWrapper>(hubProxy, target, hubConnection);
		}

		private Tuple<IHubProxy, signalSenderForTest> makeSignalSender()
		{
			return makeSignalSender(new Now());
		}
		
		private Tuple<IHubProxy, signalSenderForTest> makeSignalSender(INow now)
		{
			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.Start()).Return(_doneTask);
			var hubProxy = MockRepository.GenerateMock<IHubProxy>();
			hubProxy.Stub(x => x.Invoke("", null)).IgnoreArguments().Return(_doneTask);
			hubConnection.Stub(x => x.CreateHubProxy("MessageBrokerHub")).Return(hubProxy);
			var target = new signalSenderForTest(hubConnection, now);
			target.InstantiateBrokerService();
			return new Tuple<IHubProxy, signalSenderForTest>(hubProxy, target);
		}

		[Test]
		public void ShouldBatchNotifications()
		{
			var proxyAndSender = makeSignalSender();
			var hubProxy = proxyAndSender.Item1;
			var target = proxyAndSender.Item2;

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
			var proxyAndSender = makeSignalSender();
			var hubProxy = proxyAndSender.Item1;
			var target = proxyAndSender.Item2;

			var notifications1 = Enumerable.Range(1, 20).Select(i => new Notification()).ToArray();
			var notifications2 = Enumerable.Range(1, 10).Select(i => new Notification()).ToArray();
			notifications1.ForEach(target.SendNotificationAsync);
			notifications2.ForEach(target.SendNotificationAsync);
			target.WaitUntilAllNotificationsAreSent();

			hubProxy.AssertWasCalled(h => h.Invoke("NotifyClientsMultiple", notifications1));
			hubProxy.AssertWasCalled(h => h.Invoke("NotifyClientsMultiple", notifications2));
		}

		[Test]
		public void ShouldDiscardBatchNotificationsOlderThanTwoMinutes()
		{
			var now = new MutableNow();
			var proxyAndSender = makeSignalSender(now);
			var hubProxy = proxyAndSender.Item1;
			var target = proxyAndSender.Item2;

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
			var proxyAndSender = makeSignalSender();
			var hubProxy = proxyAndSender.Item1;
			var target = proxyAndSender.Item2;

			hubProxy.Stub(x => x.Invoke("NotifyClientsMultiple", new Notification())).Throw(new InvalidOperationException());

			Assert.DoesNotThrow(() => target.SendNotificationAsync(new Notification()));
			target.AssertWasCalled(t => t.GetBaseLogger().ErrorFormat(""), a => a.IgnoreArguments());
		}

		[Test]
		public void ShouldLogAndIgnoreOnExceptionSendingNotification()
		{
			var failedTask = makeFailedTask(new InvalidOperationException());
			var proxyAndSender = makeSignalSender();
			var hubProxy = proxyAndSender.Item1;
			var target = proxyAndSender.Item2;

			hubProxy.Stub(x => x.Invoke("NotifyClientsMultiple", new Notification())).Return(failedTask);

			Assert.DoesNotThrow(() => target.SendNotificationAsync(new Notification()));
		}
		
		[Test]
		public void ShouldRestartHubConnectionWhenConnectionClosed()
		{
		}

		[Test]
		public void ShouldThrowBrokerNotInstanciatedExceptionOnAggregateExceptionWhenStarting()
		{
			var tuple = makeNotStartedSignalSender();
			tuple.Item3.Stub(x => x.Start()).Return(makeFailedTask(new AggregateException()));
			var target = tuple.Item2;

			Assert.Throws<BrokerNotInstantiatedException>(target.InstantiateBrokerService);
		}

		[Test]
		public void ShouldThrowBrokerNotInstanciatedExceptionOnAggregateExceptionWhenInvokingStart()
		{
			var tuple = makeNotStartedSignalSender();
			tuple.Item3.Stub(x => x.Start()).Throw(new AggregateException());
			var target = tuple.Item2;

			Assert.Throws<BrokerNotInstantiatedException>(target.InstantiateBrokerService);
		}

		private static Task<object> makeFailedTask(Exception ex)
		{
			var taskCompletionSource = new TaskCompletionSource<object>();
			taskCompletionSource.SetException(ex);
			var failedTask = taskCompletionSource.Task;
			return failedTask;
		}

		[Test]
		public void ShouldThrowBrokerNotInstanciatedExceptionOnSocketExceptionWhenStarting()
		{
			var tuple = makeNotStartedSignalSender();
			tuple.Item3.Stub(x => x.Start()).Return(makeFailedTask(new SocketException()));
			var target = tuple.Item2;

			Assert.Throws<BrokerNotInstantiatedException>(target.InstantiateBrokerService);
		}

		[Test]
		public void ShouldThrowBrokerNotInstanciatedExceptionOnSocketExceptionWhenInvokingStart()
		{
			var tuple = makeNotStartedSignalSender();
			tuple.Item3.Stub(x => x.Start()).Throw(new SocketException());
			var target = tuple.Item2;

			Assert.Throws<BrokerNotInstantiatedException>(target.InstantiateBrokerService);
		}

		[Test]
		public void ShouldThrowBrokerNotInstanciatedExceptionOnInvalidOperationExceptionWhenStarting()
		{
			var tuple = makeNotStartedSignalSender();
			tuple.Item3.Stub(x => x.Start()).Return(makeFailedTask(new InvalidOperationException()));
			var target = tuple.Item2;

			Assert.Throws<BrokerNotInstantiatedException>(target.InstantiateBrokerService);
		}

		[Test]
		public void ShouldThrowBrokerNotInstanciatedExceptionOnInvalidOperationExceptionWhenInvokingStart()
		{
			var tuple = makeNotStartedSignalSender();
			tuple.Item3.Stub(x => x.Start()).Throw(new InvalidOperationException());
			var target = tuple.Item2;

			Assert.Throws<BrokerNotInstantiatedException>(target.InstantiateBrokerService);
		}

		private class signalSenderForTest : SignalSender
		{
			private readonly IHubConnectionWrapper _hubConnection;
			private readonly INow _now;

			public signalSenderForTest(IHubConnectionWrapper hubConnection, INow now)
				: base(null)
			{
				_hubConnection = hubConnection;
				_now = now;
			}

			protected override IHubConnectionWrapper MakeHubConnection()
			{
				return _hubConnection;
			}

			protected override DateTime CurrentUtcTime()
			{
				return _now.UtcDateTime();
			}

			public ILog GetBaseLogger()
			{
				return base.GetLogger();
			}
		}
	}
}
