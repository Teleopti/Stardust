using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Teleopti.Interfaces;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Messaging.Client.Http
{
	public class HttpSender : IMessageSender
	{
		private readonly IMessageBrokerUrl _url;
		private readonly IJsonSerializer _serializer;

		public Action<HttpClient, string, HttpContent> PostAsync =
			(client, uri, httpContent) =>
			{
#if DEBUG
				var task = client.PostAsync(uri, httpContent);
				var response = task.Result;
				if (response.StatusCode != HttpStatusCode.OK)
					throw new Exception(response.StatusCode.ToString());
#else
				client.PostAsync(uri, httpContent);
#endif
			};

		private readonly HttpClient _httpClient;

		public HttpSender(IMessageBrokerUrl url, IJsonSerializer seralizer)
		{
			_url = url;
			_serializer = seralizer ?? new ToStringSerializer();
			_httpClient = new HttpClient(
				new HttpClientHandler
				{
					Credentials = CredentialCache.DefaultNetworkCredentials
				});
			_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}

		public void Send(Notification notification)
		{
			call("MessageBroker/NotifyClients", notification);
		}

		public void SendMultiple(IEnumerable<Notification> notifications)
		{
			call("MessageBroker/NotifyClientsMultiple", notifications);
		}

		private void call(string call, object thing)
		{
			var content = _serializer.SerializeObject(thing);
			var u = url(call);
			PostAsync(_httpClient, u, new StringContent(content, Encoding.UTF8, "application/json"));
		}

		private string url(string call)
		{
			if (_url == null)
				return null;
			if (string.IsNullOrEmpty(_url.Url))
				return null;
			return _url.Url.TrimEnd('/') + "/" + call;
		}

	}
}