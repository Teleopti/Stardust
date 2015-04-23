using System;
using System.Dynamic;
using System.Linq;
using Castle.Core.Internal;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Web.Broker;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.WebTest.Core.MessageBroker.LegacyTests
{
	[TestFixture]
	[Ignore]
	public class MessageBrokerControllerTest
	{
		[Test]
		public void ShouldNotifyClients()
		{
			var hubContext = MockRepository.GenerateStub<IHubContext>();
			var clientsContext = MockRepository.GenerateMock<IHubConnectionContext<dynamic>>();
			hubContext.Stub(x => x.Clients).Return(clientsContext);
			var client = stubClient(clientsContext,
				"/00000000-0000-0000-0000-000000000000//id/00000000-0000-0000-0000-000000000000",
				"/00000000-0000-0000-0000-000000000000//ref/00000000-0000-0000-0000-000000000000",
				"/00000000-0000-0000-0000-000000000000/");
			var target = new MessageBrokerController(new ActionImmediate()) {HubContext = () => hubContext};

			target.NotifyClients(new Notification());

			client.MakeSureWasCalled();
		}

		[Test]
		public void ShouldNotifyAllGroupsForNotification()
		{
			var notification = new Notification();
			var hubContext = MockRepository.GenerateStub<IHubContext>();
			var clientsContext = MockRepository.GenerateMock<IHubConnectionContext<dynamic>>();
			hubContext.Stub(x => x.Clients).Return(clientsContext);

			var expects = (
				from r in notification.Routes()
				select new
				{
					client = stubClient(clientsContext,r),
					route = r
				}).ToArray();

			var target = new MessageBrokerController(new ActionImmediate()) { HubContext = () => hubContext };
			
			target.NotifyClients(notification);

			expects.ForEach(c => c.client.MakeSureWasCalledWith(notification, c.route));
		}

		[Test]
		public void ShouldNotifyClientsMultiple()
		{
			var notifications = new[] { new Notification { DataSource = "one" }, new Notification() { DataSource = "two" } };
			var hubContext = MockRepository.GenerateStub<IHubContext>();
			var clientsContext = MockRepository.GenerateMock<IHubConnectionContext<dynamic>>();
			hubContext.Stub(x => x.Clients).Return(clientsContext);

			var expects = (
				from n in notifications
				from r in n.Routes()
				let typedClient = stubClient(clientsContext, r)
				select new
				{
					client = typedClient,
					route = r,
					notification = n
				}).ToArray();

			var target = new MessageBrokerController(new ActionImmediate()) { HubContext = () => hubContext };
			
			target.NotifyClientsMultiple(notifications);

			expects.ForEach(c => c.client.MakeSureWasCalledWith(c.notification, c.route));
		}

		private dynamic stubClient(IHubConnectionContext<dynamic> clientsContext, params string[] r)
		{
			dynamic client = new ExpandoObject();
			client.onEventMessage = new Action<Notification, string>((notification, route) =>
			{
				client.wasCalled = true;
				client.Notification = notification;
				client.Route = route;
			});
			client.MakeSureWasCalledWith = new Action<Notification, string>((notification, route) =>
			{
				Assert.IsTrue(client.wasCalled);
				Assert.AreEqual(client.Notification, notification);
				Assert.AreEqual(client.Route, route);
			});
			client.MakeSureWasCalled = new Action(() => Assert.IsTrue(client.wasCalled));
			r.ForEach(v => clientsContext.Stub(x => x.Group(MessageBrokerServer.RouteToGroupName(v))).Return(client));
			return client;
		}
	}
}
