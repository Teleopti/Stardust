using System;
using log4net;
using Microsoft.AspNet.SignalR.Client;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Messaging.SignalR;
using Teleopti.Messaging.SignalR.Wrappers;

namespace Teleopti.MessagingTest.SignalR
{
	[TestFixture]
	public class SignalSenderLoggingTest
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
		public void ShouldLogWhenNotificationTasksFails()
		{
			var failedTask = TaskHelper.MakeFailedTask(new Exception());
			var hubProxy = MockRepository.GenerateMock<IHubProxyWrapper>();

			var log = MockRepository.GenerateMock<ILog>();
			var target = new SignalSenderForTest(stubHubConnection(hubProxy), log);
			target.StartBrokerService();

			hubProxy.Stub(x => x.Invoke("NotifyClientsMultiple", null)).IgnoreArguments().Return(failedTask);

			Assert.DoesNotThrow(() => target.SendNotification(new Notification()));
			log.AssertWasCalled(t => t.Debug("", null), a => a.IgnoreArguments());
		}

		[Test]
		public void ShouldLogWhenTryingToReconnect()
		{
			var hubConnection = stubHubConnection(new HubProxyFake());
			var log = MockRepository.GenerateMock<ILog>();

			var target = new SignalSenderForTest(hubConnection, log);
			target.StartBrokerService(TimeSpan.FromSeconds(0));

			hubConnection.GetEventRaiser(x => x.Closed += null).Raise();

			log.AssertWasCalled(x => x.Error(SignalConnection.ConnectionRestartedErrorMessage));
		}

		[Test]
		public void ShouldLogWhenReconnected()
		{
			var hubConnection = stubHubConnection(new HubProxyFake());
			var log = MockRepository.GenerateMock<ILog>();

			var target = new SignalSenderForTest(hubConnection, log);
			target.StartBrokerService(TimeSpan.FromSeconds(0));

			hubConnection.GetEventRaiser(x => x.Reconnected += null).Raise();

			log.AssertWasCalled(x => x.Info(SignalConnection.ConnectionReconnected));

		}
	}
}