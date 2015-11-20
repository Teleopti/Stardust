using System;
using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Web.Broker
{
	[CLSCompliant(false)]
	public class MessageBrokerController : ApiController
	{
		private readonly IMessageBrokerServer _server;

		public MessageBrokerController(IMessageBrokerServer server)
		{
			_server = server;
		}

		[HttpPost, Route("MessageBroker/NotifyClients")]
		public void NotifyClients([FromBody]Message message)
		{
			_server.NotifyClients(message);
		}

		[HttpPost, Route("MessageBroker/NotifyClientsMultiple")]
		public void NotifyClientsMultiple([FromBody]IEnumerable<Message> notifications)
		{
			_server.NotifyClientsMultiple(notifications);
		}

		[HttpGet, Route("MessageBroker/PopMessages")]
		public IHttpActionResult PopMessages(string route, string id)
		{
			try
			{
				var messages = _server.PopMessages(route, id);
				return Ok(messages);
			}
			catch (Exception)
			{
				return InternalServerError();
			}
		}
	}
}