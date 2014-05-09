using System;
using Microsoft.AspNet.SignalR.Client;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Messaging.SignalR;
using Teleopti.Messaging.SignalR.Wrappers;

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
			var hubProxy = new HubProxyFake();
			var hubConnection = stubHubConnection(hubProxy);
			var target = new SignalSenderForTest(hubConnection, null);
			target.StartBrokerService(TimeSpan.FromSeconds(0));

			hubConnection.GetEventRaiser(x => x.Closed += null).Raise();

			hubConnection.AssertWasCalled(x => x.Start(), a => a.Repeat.Twice());
		}

		[Test]
		public void ShouldNotRestartHubConnectionMoreThanXTimes()
		{
			var hubProxy = new HubProxyFake();
			var hubConnection = stubHubConnection(hubProxy);
			var target = new SignalSenderForTest(hubConnection, null);
			target.StartBrokerService(TimeSpan.FromSeconds(0), 3);

			hubConnection.GetEventRaiser(c => c.Closed += null).Raise();
			hubConnection.GetEventRaiser(c => c.Closed += null).Raise();
			hubConnection.GetEventRaiser(c => c.Closed += null).Raise();
			hubConnection.GetEventRaiser(c => c.Closed += null).Raise();

			hubConnection.AssertWasCalled(c => c.Start(), a => a.Repeat.Times(4));
		}

		[Test]
		public void ShouldContinueToRestartWhenReconnectAttemptsIsNotSet()
		{
			var hubProxy = new HubProxyFake();
			var hubConnection = stubHubConnection(hubProxy);
			var target = new SignalSenderForTest(hubConnection, null);
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
			var hubProxy = new HubProxyFake();
			var hubConnection = stubHubConnection(hubProxy);
			var target = new SignalSenderForTest(hubConnection, null);
			target.StartBrokerService(TimeSpan.FromSeconds(0));

			hubConnection.Stub(x => x.Start()).Return(TaskHelper.MakeFailedTask(new Exception())).Repeat.Once();

			hubConnection.AssertWasCalled(x => x.Start());
		}

	}
}