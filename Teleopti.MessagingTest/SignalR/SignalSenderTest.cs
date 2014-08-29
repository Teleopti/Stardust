using System.Linq;
using Microsoft.AspNet.SignalR.Client;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Messaging.Client.SignalR;
using Teleopti.Messaging.Client.SignalR.Wrappers;
using Teleopti.MessagingTest.SignalR.TestDoubles;

namespace Teleopti.MessagingTest.SignalR
{
	[TestFixture]
	public class SignalSenderTest
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
		public void ShouldSendSingleNotification()
		{
			var hubProxy = new HubProxyFake();
			var target = new SignalRClientForTest(stubHubConnection(hubProxy));
			target.StartBrokerService();
			var notification1 = new Notification();

			target.SendNotification(notification1);

			hubProxy.NotifyClientsInvokedWith.First().Should().Be(notification1);
		}

		[Test]
		public void ShouldNotSendSendWhenConnectionHasNotBeenStarted()
		{
			var hubProxy = new HubProxyFake();
			var target = new SignalRClientForTest(stubHubConnection(hubProxy));
			
			target.SendNotification(new Notification());

			hubProxy.NotifyClientsInvokedWith.Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldNotSendWhenConnectionHasStartedAndThenStopped()
		{
			var hubProxy = new HubProxyFake();
			var target = new SignalRClientForTest(stubHubConnection(hubProxy));
			target.StartBrokerService();
			target.Dispose();

			target.SendNotification(new Notification());

			hubProxy.NotifyClientsInvokedWith.Should().Have.Count.EqualTo(0);
		}
	}
}
