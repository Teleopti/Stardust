using System;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Transports;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Client.SignalR;
using Teleopti.Messaging.Client.SignalR.Wrappers;
using Teleopti.Messaging.Events;
using Teleopti.MessagingTest.SignalR.TestDoubles;

namespace Teleopti.MessagingTest.SignalR
{
	[TestFixture]
	public class SignalBrokerTest
	{
		private SignalBrokerForTest makeTarget(IHubProxyWrapper hubProxy)
		{
			var signalBroker = new SignalBrokerForTest(new MessageFilterManagerFake(), stubHubConnection(hubProxy));
			signalBroker.StartBrokerService();
			return signalBroker;
		}

		private IHubConnectionWrapper stubHubConnection(IHubProxyWrapper hubProxy)
		{
			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.State).Return(ConnectionState.Connected);
			hubConnection.Stub(x => x.Start()).Return(TaskHelper.MakeDoneTask());
			hubConnection.Stub(x => x.CreateHubProxy("MessageBrokerHub")).Return(hubProxy);
			return hubConnection;
		}

		[Test]
		public void ShouldStartLongPollingWhenSet()
		{
			var hubProxy = new HubProxyFake();

			var hubConnection = MockRepository.GenerateMock<IHubConnectionWrapper>();
			hubConnection.Stub(x => x.State).Return(ConnectionState.Connected);
			hubConnection.Stub(x => x.CreateHubProxy("MessageBrokerHub")).Return(hubProxy);
			hubConnection.Stub(x => x.Start(new LongPollingTransport())).IgnoreArguments().Return(TaskHelper.MakeDoneTask());

			var signalBroker = new SignalBrokerForTest(new MessageFilterManagerFake(), hubConnection);
			signalBroker.StartBrokerService(useLongPolling: true);

			hubConnection.AssertWasCalled(x => x.Start(new LongPollingTransport()), o => o.Constraints(Rhino.Mocks.Constraints.Is.TypeOf<LongPollingTransport>()));
		}

		[Test]
		public void ShouldSendEventMessage()
		{
			var hubProxy = new HubProxyFake();
			var target = makeTarget(hubProxy);

			target.Send(string.Empty, Guid.Empty, DateTime.UtcNow, DateTime.UtcNow, Guid.Empty, Guid.Empty,
									typeof(string), DomainUpdateType.Update, new byte[] { });

			hubProxy.NotifyClientsMultipleInvokedWith.Should().Have.Count.GreaterThan(0);
		}

		[Test]
		public void ShouldSendNotification()
		{
			var hubProxy = new HubProxyFake();
			var target = makeTarget(hubProxy);

			target.Send(new Notification());

			hubProxy.NotifyClientsInvokedWith.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldSendBatchEventMessages()
		{
			var hubProxy = new HubProxyFake();
			var target = makeTarget(hubProxy);

			target.Send(string.Empty, Guid.Empty,
									 new IEventMessage[]
				                         {
					                         new EventMessage {DomainObjectType = typeof(string).AssemblyQualifiedName},
					                         new EventMessage {DomainObjectType = typeof(string).AssemblyQualifiedName}
				                         });

			hubProxy.NotifyClientsMultipleInvokedWith.Should().Have.Count.GreaterThan(0);
		}

	}
}
