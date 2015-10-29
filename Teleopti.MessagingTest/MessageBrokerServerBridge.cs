using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Messaging.Client.Http;

namespace Teleopti.MessagingTest
{
	public class MessageBrokerServerBridge : IHttpServer
	{
		private readonly IMessageBrokerServer _server;
		private Exception _exception;
		public readonly IList<RequestInfo> Requests = new List<RequestInfo>();

		public MessageBrokerServerBridge(IMessageBrokerServer server)
		{
			_server = server;
		}
		
		public void Receives(Message message)
		{
			_server.NotifyClients(message);
		}
		public void Post(HttpClient client, string uri, HttpContent httpContent)
		{
			Requests.Add(new RequestInfo { Client = client, Uri = uri, HttpContent = httpContent });
		}

		public void PostOrThrow(HttpClient client, string uri, HttpContent httpContent)
		{
			Requests.Add(new RequestInfo { Client = client, Uri = uri, HttpContent = httpContent });
			if (_exception != null)
				throw _exception;
		}

		public string GetOrThrow(HttpClient client, string uri)
		{
			Requests.Add(new RequestInfo { Client = client, Uri = uri });

			if (_exception != null)
				throw _exception;

			if (uri.Contains("MessageBroker/PopMessages"))
			{
				var values = HttpUtility.ParseQueryString(new Uri(uri).Query);
				var messages = _server.PopMessages(values["route"], values["id"]);
				return JsonConvert.SerializeObject(messages);
			}

			return null;
		}

		public void Down()
		{
			Throws(new AggregateException(new HttpRequestException()));
		}

		public void IsSlow()
		{
			Throws(new AggregateException(new TaskCanceledException()));
		}

		public void GivesError(HttpStatusCode httpCode)
		{
			Throws(new HttpRequestException());
		}

		public void IsHappy()
		{
			Throws(null);
		}

		public void Throws(Exception type)
		{
			_exception = type;
		}


	}
}