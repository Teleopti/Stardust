using System;
using Microsoft.AspNet.SignalR.Client;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon;
using Teleopti.Messaging.Client.SignalR;
using Teleopti.Messaging.Client.SignalR.Wrappers;
using Teleopti.MessagingTest.SignalR.TestDoubles;

namespace Teleopti.MessagingTest.SignalR
{
	[TestFixture]
	public class SignalConnectionRestartOnClosedTest
	{
		private IHubConnectionWrapper stubHubConnection(IHubProxyWrapper hubProxy)
		{
			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.State).Return(ConnectionState.Connected);
			hubConnection.Stub(x => x.Start()).Return(TaskHelper.MakeDoneTask());
			hubConnection.Stub(x => x.CreateHubProxy("MessageBrokerHub")).Return(hubProxy);
			return hubConnection;
		}

		[Test]
		public void ShouldRestartHubConnectionWhenConnectionClosed()
		{
			var time = new FakeTime();
			var hubProxy = new HubProxyFake();
			var hubConnection = stubHubConnection(hubProxy);
			var target = new SignalRClientForTest(hubConnection, new RestartOnClosed(TimeSpan.FromSeconds(4)), time);
			target.StartBrokerService();

			hubConnection.GetEventRaiser(x => x.Closed += null).Raise();
			time.Passes(TimeSpan.FromMinutes(5));

			hubConnection.AssertWasCalled(x => x.Start(), a => a.Repeat.Twice());
		}

		[Test]
		public void ShouldContinueToRestartHubConnectionWhenConnectionClosed()
		{
			var time = new FakeTime();
			var hubProxy = new HubProxyFake();
			var hubConnection = stubHubConnection(hubProxy);
			var target = new SignalRClientForTest(hubConnection, new RestartOnClosed(TimeSpan.FromSeconds(4)), time);
			target.StartBrokerService();

			hubConnection.GetEventRaiser(c => c.Closed += null).Raise();
			time.Passes(TimeSpan.FromMinutes(5));
			hubConnection.GetEventRaiser(c => c.Closed += null).Raise();
			time.Passes(TimeSpan.FromMinutes(5));
			hubConnection.GetEventRaiser(c => c.Closed += null).Raise();
			time.Passes(TimeSpan.FromMinutes(5));
			hubConnection.GetEventRaiser(c => c.Closed += null).Raise();
			time.Passes(TimeSpan.FromMinutes(5));
			hubConnection.GetEventRaiser(c => c.Closed += null).Raise();
			time.Passes(TimeSpan.FromMinutes(5));

			hubConnection.AssertWasCalled(c => c.Start(), a => a.Repeat.Times(6));
		}

		[Test]
		public void ShouldStopRestartingWhenDisposed()
		{
			var time = new FakeTime();
			var hubProxy = new HubProxyFake();
			var hubConnection = stubHubConnection(hubProxy);
			var target = new SignalRClientForTest(hubConnection, new RestartOnClosed(TimeSpan.FromSeconds(4)), time);
			target.StartBrokerService();
			target.Dispose();

			hubConnection.GetEventRaiser(x => x.Closed += null).Raise();
			time.Passes(TimeSpan.FromMinutes(5));

			hubConnection.AssertWasCalled(x => x.Start(), a => a.Repeat.Once());
		}

		[Test, Ignore("This does not test anything, and we dont know if we want this behavior")]
		public void ShouldRestartHubConnectionWhenStartFails()
		{
			var time = new FakeTime();
			var hubProxy = new HubProxyFake();
			var hubConnection = stubHubConnection(hubProxy);
			var target = new SignalRClientForTest(hubConnection, new RestartOnClosed(TimeSpan.FromSeconds(0)), time);
			target.StartBrokerService();

			hubConnection.Stub(x => x.Start()).Return(TaskHelper.MakeFailedTask(new Exception())).Repeat.Once();

			hubConnection.AssertWasCalled(x => x.Start());
		}

		[Test]
		public void ShouldUseLongPollingWhenReconnectingIfStartedWithLongPolling()
		{
			var time = new FakeTime();
			var hubProxy = new HubProxyFake();
			var hubConnection = new HubConnectionMock(hubProxy);
			var target = new SignalRClientForTest(hubConnection, new RestartOnClosed(TimeSpan.FromSeconds(4)), time);
			target.StartBrokerService(useLongPolling: true);

			hubConnection.RaiseClosedEvent();
			time.Passes(TimeSpan.FromMinutes(5));

			hubConnection.NumberOfTimesStartWithTransportWasCalled.Should().Be.EqualTo(2);
		}

	}
}