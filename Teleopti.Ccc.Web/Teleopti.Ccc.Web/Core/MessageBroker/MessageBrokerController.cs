using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Web.Broker
{
	[CLSCompliant(false)]
	public class MessageBrokerController : Controller
	{
		private readonly IMessageBrokerServer _server;

		public MessageBrokerController(IMessageBrokerServer server)
		{
			_server = server;
		}

		public void NotifyClients(Message message)
		{
			_server.NotifyClients(message);
		}

		public void NotifyClientsMultiple(IEnumerable<Message> notifications)
		{
			_server.NotifyClientsMultiple(notifications);
		}

		public void AddMailbox(Subscription subscription)
		{
			_server.AddMailbox(subscription);
		}

		public JsonResult PopMessages(string id)
		{
			return Json(_server.PopMessages(id), JsonRequestBehavior.AllowGet);
		}
	}
}