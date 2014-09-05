using System.Linq;
using Castle.Core.Internal;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Web.Broker;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.WebTest.Core.MessageBroker
{
	public class MessageBrokerControllerTest
	{
		[Test]
		public void ShouldNotifyClients()
		{
			var hubContext = MockRepository.GenerateStub<IHubContext>();
			var clientsContext = MockRepository.GenerateMock<IHubConnectionContext>();
			hubContext.Stub(x => x.Clients).Return(clientsContext);
			var client = MockRepository.GenerateMock<IOnEventMessageClient>();
			clientsContext.Stub(x => x.Group(null)).IgnoreArguments().Return(client);
			var target = new MessageBrokerController(new ActionImmediate()) {HubContext = () => hubContext};

			target.NotifyClients(new Notification());
			
			client.AssertWasCalled(x => x.onEventMessage(null, null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldNotifyAllGroupsForNotification()
		{
			var notification = new Notification();
			var hubContext = MockRepository.GenerateStub<IHubContext>();
			var clientsContext = MockRepository.GenerateMock<IHubConnectionContext>();
			hubContext.Stub(x => x.Clients).Return(clientsContext);

			var expects = (
				from r in notification.Routes()
				let client = MockRepository.GenerateMock<IOnEventMessageClient>()
				select new
				{
					client = stubClient(r, clientsContext),
					route = r
				}).ToArray();

			var target = new MessageBrokerController(new ActionImmediate()) { HubContext = () => hubContext };
			
			target.NotifyClients(notification);

			expects.ForEach(c =>
				c.client.AssertWasCalled(x => x.onEventMessage(notification, c.route))
				);
		}

		[Test]
		public void ShouldNotifyClientsMultiple()
		{
			var notifications = new[] { new Notification() { DataSource = "one" }, new Notification() { DataSource = "two" } };
			var hubContext = MockRepository.GenerateStub<IHubContext>();
			var clientsContext = MockRepository.GenerateMock<IHubConnectionContext>();
			hubContext.Stub(x => x.Clients).Return(clientsContext);

			var expects = (
				from n in notifications
				from r in n.Routes()
				select new
				{
					client = stubClient(r, clientsContext),
					route = r,
					notification = n
				}).ToArray();

			var target = new MessageBrokerController(new ActionImmediate()) { HubContext = () => hubContext };

			target.NotifyClientsMultiple(notifications);

			expects.ForEach(c =>
				c.client.AssertWasCalled(x => x.onEventMessage(c.notification, c.route))
				);
		}

		private IOnEventMessageClient stubClient(string r, IHubConnectionContext clientsContext)
		{
			var client = MockRepository.GenerateMock<IOnEventMessageClient>();
			clientsContext.Stub(x => x.Group(MessageBrokerServer.RouteToGroupName(r))).Return(client);
			return client;
		}

		public interface IOnEventMessageClient
		{
			void onEventMessage(Notification notification, string route);
		}
	}
}
