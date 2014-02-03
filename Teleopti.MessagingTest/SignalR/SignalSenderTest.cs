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
		public void ShouldSendRtaNotification()
		{
			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.Start()).Return(_doneTask);
			var hubProxy = MockRepository.GenerateMock<IHubProxy>();
			hubProxy.Stub(x => x.Invoke("", null)).IgnoreArguments().Return(_doneTask);
			hubConnection.Stub(x => x.CreateHubProxy("MessageBrokerHub")).Return(hubProxy);
			var target = new signalSenderForTest(hubConnection);
			target.InstantiateBrokerService();
			target.QueueRtaNotification(Guid.Empty, Guid.Empty, new ActualAgentState());
			target.WaitUntilQueueProcessed();

			hubProxy.AssertWasCalled(
				h =>
				h.Invoke(Arg<string>.Is.Equal("NotifyClientsMultiple"), 
				Arg<IEnumerable<Notification>>.List.Count(Rhino.Mocks.Constraints.Is.Equal(1))));
		}

		[Test]
		public void ShouldRetryFailedNotification()
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
			var target = new signalSenderForTest(hubConnection);
			target.InstantiateBrokerService();
			target.QueueRtaNotification(Guid.Empty, Guid.Empty, new ActualAgentState());
			target.WaitUntilQueueProcessed();

			hubProxy.AssertWasCalled(
				h =>
				h.Invoke(Arg<string>.Is.Equal("NotifyClientsMultiple"),
				Arg<IEnumerable<Notification>>.List.Count(Rhino.Mocks.Constraints.Is.Equal(1))), a => a.Repeat.Twice());
		}

		[Test]
		public void ShouldIgnoreAfterThreeRetries()
		{
			var taskCompletionSource = new TaskCompletionSource<object>();
			taskCompletionSource.SetException(new InvalidOperationException());
			var failedTask = taskCompletionSource.Task;

			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.Start()).Return(_doneTask);
			var hubProxy = MockRepository.GenerateMock<IHubProxy>();
			hubProxy.Stub(x => x.Invoke("", null)).IgnoreArguments().Repeat.Times(3).Return(failedTask);
			hubConnection.Stub(x => x.CreateHubProxy("MessageBrokerHub")).Return(hubProxy);
			var target = new signalSenderForTest(hubConnection);
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
		public void ShouldDiscardNotificationsOlderThanTwoMinutes()
		{
			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.Start()).Return(_doneTask);
			var hubProxy = MockRepository.GenerateMock<IHubProxy>();
			hubProxy.Expect(x => x.Invoke("", null)).IgnoreArguments().Repeat.Times(3).Return(_doneTask);
			hubConnection.Stub(x => x.CreateHubProxy("MessageBrokerHub")).Return(hubProxy);
			var target = new signalSenderForTest(hubConnection);
			target.InstantiateBrokerService();
			target.QueueRtaNotification(Guid.Empty, Guid.Empty, new ActualAgentState());
			target.WaitUntilQueueProcessed();

			hubProxy.AssertWasNotCalled(
				h =>
				h.Invoke(Arg<string>.Is.Equal("NotifyClientsMultiple"),
				Arg<IEnumerable<Notification>>.List));
		}

		[Test]
		public void ShouldStopOnDispose()
		{
			Assert.Ignore("Test describing existing functionality, implement in future feature");
		}
		
		[Test]
		public void ShouldMakeNewConnectionOnSendExceptionWhileSending_Really()
		{
			Assert.Ignore("Test describing existing functionality");
		}

		[Test]
		public void ShouldIgnoreExceptionsWhenInvoking_Really()
		{
			Assert.Ignore("Test describing existing functionality");
		}
		
		[Test]
		public void ShouldForceRestartHubConnectionWhenNotConnected_Really()
		{
			Assert.Ignore("Test describing existing functionality");
		}

		[Test]
		public void ShouldThrowBrokerNotInstanciatedWhenFailedForceReconnect_Really()
		{
			Assert.Ignore("Test describing existing functionality");
		}

		
		//[Test]
		//public void SendData_ShouldSend()
		//{
		//	var wrapperMock = MockRepository.GenerateMock<ISignalWrapper>();
		//	var target = new signalSenderExposer {SignalWrapper = wrapperMock};

		//	wrapperMock.Expect(w => w.NotifyClients(new Notification())).IgnoreArguments().Return(_emptyTask);
		//	target.SendData(DateTime.Today, DateTime.Today, Guid.Empty, Guid.Empty, typeof(object), "", Guid.Empty);

		//	wrapperMock.AssertWasCalled(w => w.NotifyClients(new Notification()), a => a.IgnoreArguments());
		//}

		//[Test]
		//public void ShouldConnect()
		//{
		//	var wrapperMock = MockRepository.GenerateMock<ISignalWrapper>();
		//	var target = new signalSenderExposer {SignalWrapper = wrapperMock};

		//	target.InstantiateBrokerService();
		//	wrapperMock.AssertWasCalled(w => w.StopHub());
			
		//}

		private class signalSenderForTest : SignalSender
		{
			private readonly IHubConnectionWrapper _hubConnection;

			public signalSenderForTest(IHubConnectionWrapper hubConnection)
				: base(null)
			{
				_hubConnection = hubConnection;
			}

			protected override IHubConnectionWrapper MakeHubConnection()
			{
				return _hubConnection;
			}
		}
	}
}
