using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Server;

namespace Teleopti.Ccc.TestCommon.Messaging
{
	public class MessageBrokerServerBridge : IHttpServer
	{
		private readonly IMessageBrokerServer _server;
		private Exception _exception;
		public readonly IList<RequestInfo> Requests = new List<RequestInfo>();
		private bool _downFriendly;

		public MessageBrokerServerBridge(IMessageBrokerServer server)
		{
			_server = server;
		}
		
		public void Receives(Message message)
		{
			_server.NotifyClients(message);
		}

		public Task<HttpResponseMessage> Post(string uri, object data, Func<string, NameValueCollection> customHeadersFunc = null)
		{
			var headers = customHeadersFunc?.Invoke(data.ToString());
			Requests.Add(new RequestInfo { Uri = uri, Data = data, Headers = headers });
			return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
		}

		public void PostOrThrow(string uri, object data, Func<string, NameValueCollection> customHeadersFunc = null)
		{
			var headers = customHeadersFunc?.Invoke(data.ToString());
			Requests.Add(new RequestInfo { Uri = uri, Data = data, Headers = headers });
			if (_exception != null)
				throw _exception;
		}

		public Task PostOrThrowAsync(string uri, object data, Func<string, NameValueCollection> customHeadersFunc = null)
		{
			var headers = customHeadersFunc?.Invoke(data.ToString());
			Requests.Add(new RequestInfo { Uri = uri, Data = data, Headers = headers });
			if (_exception != null)
				return Task.FromException(_exception);

			return Task.FromResult(false);
		}

		public string GetOrThrow(string uri)
		{
			Requests.Add(new RequestInfo { Uri = uri });

			if (_downFriendly)
				return "<html><body>A 'friendly' response</body></html>";
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

		public void DownFriendly()
		{
			_downFriendly = true;
		}

		public void Down()
		{
			Throws(new System.AggregateException(new HttpRequestException()));
		}

		public void IsSlow()
		{
			Throws(new System.AggregateException(new TaskCanceledException()));
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