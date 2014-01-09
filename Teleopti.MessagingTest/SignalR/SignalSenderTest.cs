using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client.Hubs;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
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

		//[Test]
		//public void QueueRtaNotification_ShouldQueue()
		//{
		//	var personId = Guid.NewGuid();
		//	var businessUnitId = Guid.NewGuid();
		//	var actualAgentState = new ActualAgentState();

		//	var target = new signalSenderExposer();
		//	target.WorkerThread.Abort();
		//	target.QueueRtaNotification(personId, businessUnitId, actualAgentState);
			
		//	target.NotificationQueue.Count.Should().Be(1);
		//}

		//[Test]
		//public void BackgroundWorkerPicksUpItems()
		//{
		//	var personId = Guid.NewGuid();
		//	var businessUnitId = Guid.NewGuid();
		//	var actualAgentState = new ActualAgentState();
		//	var wrapperMock = MockRepository.GenerateMock<ISignalWrapper>();

		//	var target = new signalSenderExposer {SignalWrapper = wrapperMock};
		//	target.QueueRtaNotification(personId, businessUnitId, actualAgentState);
			
		//	wrapperMock.AssertWasCalled(w => w.NotifyClients(new[] {new Notification()}), a => a.IgnoreArguments());
		//}

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
		//public void InstantiateBrokerService()
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

			//public ISignalWrapper SignalWrapper
			//{
			//	set { _wrapper = value; }
			//}

			//public BlockingCollection<Tuple<DateTime, Notification>> NotificationQueue
			//{
			//	get { return _notificationQueue; }
			//}

			//public Thread WorkerThread
			//{
			//	get { return workerThread; }
			//}

		}
	}
}
