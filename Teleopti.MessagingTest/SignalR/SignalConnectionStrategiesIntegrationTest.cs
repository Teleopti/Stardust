using System;
using Microsoft.AspNet.SignalR.Client;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.TestCommon;
using Teleopti.Messaging.SignalR;
using Teleopti.Messaging.SignalR.Wrappers;
using Teleopti.MessagingTest.SignalR.TestDoubles;

namespace Teleopti.MessagingTest.SignalR
{
	[TestFixture]
	public class SignalConnectionStrategiesIntegrationTest
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
		public void ShouldRestartHubConnectionWhenRecreatedConnectionClosed()
		{
			var time = new FakeTime();
			var hubProxy1 = new HubProxyFake();
			var hubProxy2 = new HubProxyFake();
			var hubConnection1 = stubHubConnection(hubProxy1);
			var hubConnection2 = stubHubConnection(hubProxy2);
			var target = new MultiConnectionSignalRClientForTest(new[] {hubConnection1, hubConnection2},
				new IConnectionKeepAliveStrategy[] {new RestartOnClosed(TimeSpan.FromMinutes(1)), new RecreateOnNoPingReply(TimeSpan.FromMinutes(1))}, time);
			target.StartBrokerService();

			hubProxy1.BreakTheConnection();
			time.Passes(TimeSpan.FromMinutes(2));

			hubConnection2.GetEventRaiser(x => x.Closed += null).Raise();
			time.Passes(TimeSpan.FromMinutes(2));

			hubConnection2.AssertWasCalled(x => x.Start(), a => a.Repeat.Twice());
		}

		[Test]
		public void ShouldNotRestartPreviousHubConnectionWhenConnectionHasBeenRecreated()
		{
			var time = new FakeTime();
			var hubProxy1 = new HubProxyFake();
			var hubProxy2 = new HubProxyFake();
			var hubConnection1 = stubHubConnection(hubProxy1);
			var hubConnection2 = stubHubConnection(hubProxy2);
			var target = new MultiConnectionSignalRClientForTest(new[] { hubConnection1, hubConnection2 },
				new IConnectionKeepAliveStrategy[] { new RestartOnClosed(TimeSpan.FromMinutes(2)), new RecreateOnNoPingReply(TimeSpan.FromMinutes(1)) }, time);
			target.StartBrokerService();

			hubProxy1.BreakTheConnection();
			hubConnection1.GetEventRaiser(x => x.Closed += null).Raise();
			time.Passes(TimeSpan.FromMinutes(10));

			hubConnection1.AssertWasCalled(x => x.Start(), a => a.Repeat.Once());
			hubConnection2.AssertWasCalled(x => x.Start(), a => a.Repeat.Once());
		}

	}
}