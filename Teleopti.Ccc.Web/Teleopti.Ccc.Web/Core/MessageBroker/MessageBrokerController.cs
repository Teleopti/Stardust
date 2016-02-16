using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Server;

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
		public void NotifyClientsMultiple([FromBody]Message[] notifications)
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
	/*
	public class MessageBinder : IModelBinder
	{
		public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
		{
			if (bindingContext.ModelType != typeof(Message))
			{
				return false;
			}

			string DataSource { get; set; }
		string BusinessUnitId { get; set; }

		string DomainType { get; set; }
		string DomainQualifiedType { get; set; }
		string DomainId { get; set; }
		string ModuleId { get; set; }
		string DomainReferenceId { get; set; }
		string EndDate { get; set; }
		string StartDate { get; set; }
		int DomainUpdateType { get; set; }
		string BinaryData { get; set; }

			ValueProviderResult username = bindingContext.ValueProvider.GetValue("username");
			ValueProviderResult password = bindingContext.ValueProvider.GetValue("password");
			if (username == null || password == null)
			{
				return false;
			}

			if (username.RawValue == null || password.RawValue == null)
			{
				bindingContext.ModelState.AddModelError(
					bindingContext.ModelName, "Cannot convert value to application authentication");
				return false;
			}

			bindingContext.Model = new ApplicationAuthenticationModel { UserName = username.RawValue.ToString(), Password = password.RawValue.ToString() };
			return true;
		}
	}*/
}