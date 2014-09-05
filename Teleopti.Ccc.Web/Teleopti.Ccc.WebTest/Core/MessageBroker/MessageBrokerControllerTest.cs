using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Castle.Core.Internal;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using SharpTestsEx;
using Teleopti.Ccc.Web.Broker;
using Teleopti.Ccc.WebTest.Areas.Anywhere.Hubs;
using Teleopti.Interfaces.MessageBroker;
using Extensions = Teleopti.Ccc.Domain.Collection.Extensions;

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
			var target = new MessageBrokerController(() => hubContext);

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

			var target = new MessageBrokerController(() => hubContext);
			
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

			var target = new MessageBrokerController(() => hubContext);

			target.NotifyClientsMultiple(notifications);

			expects.ForEach(c =>
				c.client.AssertWasCalled(x => x.onEventMessage(c.notification, c.route))
				);
		}

		private IOnEventMessageClient stubClient(string r, IHubConnectionContext clientsContext)
		{
			var client = MockRepository.GenerateMock<IOnEventMessageClient>();
			clientsContext.Stub(x => x.Group(MessageBrokerHub.RouteToGroupName(r))).Return(client);
			return client;
		}

		public interface IOnEventMessageClient
		{
			void onEventMessage(Notification notification, string route);
		}
	}

	public class MessageBrokerController
	{
		private readonly Func<IHubContext> _hubContext;

		public MessageBrokerController(Func<IHubContext> hubContext)
		{
			_hubContext = hubContext;
		}

		public void NotifyClients(Notification notification)
		{
			notification.Routes().ForEach(r =>
				_hubContext().Clients.Group(MessageBrokerHub.RouteToGroupName(r)).onEventMessage(notification, r)
				);
		}

		public void NotifyClientsMultiple(IEnumerable<Notification> notifications)
		{
			notifications.ForEach(NotifyClients);
		}
	}
}
