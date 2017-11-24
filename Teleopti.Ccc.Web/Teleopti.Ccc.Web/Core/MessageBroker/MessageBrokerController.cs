using System;
using System.Web.Http;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Server;

namespace Teleopti.Ccc.Web.Broker
{
	public class MessageBrokerController : ApiController
	{
		private readonly IMessageBrokerServer _server;

		public MessageBrokerController(IMessageBrokerServer server)
		{
			_server = server;
		}

		[System.Web.Http.HttpPost, System.Web.Http.Route("MessageBroker/NotifyClients")]
		public void NotifyClients([FromBody] Message message)
		{
			_server.NotifyClients(message);
		}

		[System.Web.Http.HttpPost, System.Web.Http.Route("MessageBroker/NotifyClientsMultiple")]
		public void NotifyClientsMultiple([FromBody] Message[] notifications)
		{
			_server.NotifyClientsMultiple(notifications);
		}

		[System.Web.Http.HttpGet, System.Web.Http.Route("MessageBroker/PopMessages")]
		public IHttpActionResult PopMessages(string route, string id)
		{
			return Ok(_server.PopMessages(route, id));
		}
	}
}