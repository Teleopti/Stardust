using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client.Hubs;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Messaging.SignalR;

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

		[Test]
		public void ShouldSendBatchNotification()
		{
			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.Start()).Return(_doneTask);
			var hubProxy = MockRepository.GenerateMock<IHubProxy>();
			hubProxy.Stub(x => x.Invoke("", null)).IgnoreArguments().Return(_doneTask);
			hubConnection.Stub(x => x.CreateHubProxy("MessageBrokerHub")).Return(hubProxy);
			var target = new signalSenderForTest(hubConnection, DateTime.UtcNow);
			target.InstantiateBrokerService();
			target.QueueRtaNotification(Guid.Empty, Guid.Empty, new ActualAgentState());
			target.WaitUntilQueueProcessed();

			hubProxy.AssertWasCalled(
				h =>
				h.Invoke(Arg<string>.Is.Equal("NotifyClientsMultiple"), 
				Arg<IEnumerable<Notification>>.List.Count(Rhino.Mocks.Constraints.Is.Equal(1))));
		}

		[Test]
		public void ShouldRetryFailedBatchNotification()
		{
			var taskCompletionSource = new TaskCompletionSource<object>();
			taskCompletionSource.SetException(new InvalidOperationException());
			var failedTask = taskCompletionSource.Task;

			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.Start()).Return(_doneTask);
			var hubProxy = MockRepository.GenerateMock<IHubProxy>();
			hubProxy.Stub(x => x.Invoke("", null)).IgnoreArguments().Repeat.Once().Return(failedTask);
			hubProxy.Stub(x => x.Invoke("", null)).IgnoreArguments().Repeat.Once().Return(_doneTask);
			hubConnection.Stub(x => x.CreateHubProxy("MessageBrokerHub")).Return(hubProxy);
			var target = new signalSenderForTest(hubConnection, DateTime.UtcNow);
			target.InstantiateBrokerService();
			target.QueueRtaNotification(Guid.Empty, Guid.Empty, new ActualAgentState());
			target.WaitUntilQueueProcessed();

			hubProxy.AssertWasCalled(
				h =>
				h.Invoke(Arg<string>.Is.Equal("NotifyClientsMultiple"),
				Arg<IEnumerable<Notification>>.List.Count(Rhino.Mocks.Constraints.Is.Equal(1))), a => a.Repeat.Twice());
		}

		[Test]
		public void ShouldIgnoreBatchAfterThreeRetries()
		{
			var taskCompletionSource = new TaskCompletionSource<object>();
			taskCompletionSource.SetException(new InvalidOperationException());
			var failedTask = taskCompletionSource.Task;

			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.Start()).Return(_doneTask);
			var hubProxy = MockRepository.GenerateMock<IHubProxy>();
			hubProxy.Stub(x => x.Invoke("", null)).IgnoreArguments().Repeat.Times(3).Return(failedTask);
			hubConnection.Stub(x => x.CreateHubProxy("MessageBrokerHub")).Return(hubProxy);
			var target = new signalSenderForTest(hubConnection, DateTime.UtcNow);
			target.InstantiateBrokerService();
			target.QueueRtaNotification(Guid.Empty, Guid.Empty, new ActualAgentState());
			target.WaitUntilQueueProcessed();

			hubProxy.AssertWasCalled(
				h =>
				h.Invoke(Arg<string>.Is.Equal("NotifyClientsMultiple"),
				Arg<IEnumerable<Notification>>.List.Count(Rhino.Mocks.Constraints.Is.Equal(1))), a => a.Repeat.Times(3));
		}

		[Test]
		public void ShouldBatchTwentyNotificationsAtATime()
		{
			Assert.Ignore("Test describing existing functionality, implement in future feature");
		}

		[Test]
		public void ShouldDiscardBatchNotificationsOlderThanTwoMinutes()
		{
			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.Start()).Return(_doneTask);
			var hubProxy = MockRepository.GenerateMock<IHubProxy>();
			hubProxy.Expect(x => x.Invoke("", null)).IgnoreArguments().Return(_doneTask);
			hubConnection.Stub(x => x.CreateHubProxy("MessageBrokerHub")).Return(hubProxy);
			var target = new signalSenderForTest(hubConnection, DateTime.UtcNow.AddMinutes(-2));
			target.InstantiateBrokerService();
			target.QueueRtaNotification(Guid.Empty, Guid.Empty, new ActualAgentState());
			target.WaitUntilQueueProcessed();
			
			hubProxy.AssertWasNotCalled(
				h =>
				h.Invoke(Arg<string>.Is.Equal("NotifyClientsMultiple"), 
				Arg<IEnumerable<Notification>>.Is.Anything));
		}

		[Test]
		public void ShouldStopSendingBatchOnDispose()
		{
			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.Start()).Return(_doneTask);
			var hubProxy = MockRepository.GenerateMock<IHubProxy>();
			hubProxy.Expect(x => x.Invoke("", null)).IgnoreArguments().Return(_doneTask);
			hubConnection.Stub(x => x.CreateHubProxy("MessageBrokerHub")).Return(hubProxy);
			var target = new signalSenderForTest(hubConnection, DateTime.UtcNow);
			target.InstantiateBrokerService();
			target.Dispose(); 
			target.QueueRtaNotification(Guid.Empty, Guid.Empty, new ActualAgentState());

			hubProxy.AssertWasNotCalled(
				h =>
				h.Invoke(Arg<string>.Is.Equal("NotifyClientsMultiple"), Arg<IEnumerable<Notification>>.Is.Anything));
		}


		[Test]
		public void ShouldSendSingleNotification()
		{
			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.Start()).Return(_doneTask);
			var hubProxy = MockRepository.GenerateMock<IHubProxy>();
			hubProxy.Expect(x => x.Invoke("", null)).IgnoreArguments().Repeat.Times(3).Return(_doneTask);
			hubConnection.Stub(x => x.CreateHubProxy("MessageBrokerHub")).Return(hubProxy);
			var target = new signalSenderForTest(hubConnection, DateTime.UtcNow);
			target.InstantiateBrokerService();
			target.SendData(DateTime.UtcNow, DateTime.UtcNow, Guid.Empty, Guid.Empty, typeof(string), string.Empty, Guid.Empty);
			
			hubProxy.AssertWasCalled(
				h =>
				h.Invoke(Arg<string>.Is.Equal("NotifyClients"), Arg<Notification>.Is.Anything));
		}

		[Test]
		public void ShouldRetryFailedSingleNotification()
		{
			var taskCompletionSource = new TaskCompletionSource<object>();
			taskCompletionSource.SetException(new InvalidOperationException());
			var failedTask = taskCompletionSource.Task;

			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.Start()).Return(_doneTask);
			var hubProxy = MockRepository.GenerateMock<IHubProxy>();
			hubProxy.Stub(x => x.Invoke("", null)).IgnoreArguments().Repeat.Once().Return(failedTask);
			hubProxy.Stub(x => x.Invoke("", null)).IgnoreArguments().Repeat.Once().Return(_doneTask);
			hubConnection.Stub(x => x.CreateHubProxy("MessageBrokerHub")).Return(hubProxy);
			var target = new signalSenderForTest(hubConnection, DateTime.UtcNow);
			target.InstantiateBrokerService();
			target.SendData(DateTime.UtcNow, DateTime.UtcNow, Guid.Empty, Guid.Empty, typeof(string), string.Empty, Guid.Empty);

			hubProxy.AssertWasCalled(
				h =>
				h.Invoke(Arg<string>.Is.Equal("NotifyClients"),
				Arg<Notification>.Is.Anything), a => a.Repeat.Twice());
		}

		[Test]
		public void ShouldIgnoreSingleNotificationAfterThreeRetries()
		{
			var taskCompletionSource = new TaskCompletionSource<object>();
			taskCompletionSource.SetException(new InvalidOperationException());
			var failedTask = taskCompletionSource.Task;

			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.Start()).Return(_doneTask);
			var hubProxy = MockRepository.GenerateMock<IHubProxy>();
			hubProxy.Stub(x => x.Invoke("", null)).IgnoreArguments().Repeat.Times(3).Return(failedTask);
			hubConnection.Stub(x => x.CreateHubProxy("MessageBrokerHub")).Return(hubProxy);
			var target = new signalSenderForTest(hubConnection, DateTime.UtcNow);
			target.InstantiateBrokerService();
			target.SendData(DateTime.UtcNow, DateTime.UtcNow, Guid.Empty, Guid.Empty, typeof(string), string.Empty, Guid.Empty);

			hubProxy.AssertWasCalled(
				h =>
				h.Invoke(Arg<string>.Is.Equal("NotifyClients"),
				Arg<Notification>.Is.Anything), a => a.Repeat.Times(3));
		}
		
		[Test]
		public void Really_ShouldMakeNewConnectionOnSendExceptionWhileSending()
		{
			Assert.Ignore("Test describing existing functionality");
		}

		[Test]
		public void Really_ShouldIgnoreExceptionsWhenInvoking()
		{
			Assert.Ignore("Test describing existing functionality");
		}
		
		[Test]
		public void Really_ShouldForceRestartHubConnectionWhenNotConnected()
		{
			Assert.Ignore("Test describing existing functionality");
		}

		[Test]
		public void Really_ShouldThrowBrokerNotInstanciatedWhenFailedForceReconnect()
		{
			Assert.Ignore("Test describing existing functionality");
		}

		private class signalSenderForTest : SignalSender
		{
			private readonly IHubConnectionWrapper _hubConnection;
			private readonly DateTime _fakedUtcTime;

			public signalSenderForTest(IHubConnectionWrapper hubConnection, DateTime fakedUtcTime)
				: base(null)
			{
				_hubConnection = hubConnection;
				_fakedUtcTime = fakedUtcTime;
			}

			protected override IHubConnectionWrapper MakeHubConnection()
			{
				return _hubConnection;
			}

			protected override DateTime CurrentUtcTime()
			{
				return _fakedUtcTime;
			}
		}
	}
}
