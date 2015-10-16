using System;
using System.Net;
using System.Net.Http;
using System.Text;
using Teleopti.Interfaces;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Messaging.Client.Http
{
	// MMMMmmMm as in Mathias.
	// No, seriously, just couldnt use HttpClient and did want to call it HttpClient2 ROFL
	public class HttpClientM
	{
		private readonly IHttpServer _server;
		private readonly IMessageBrokerUrl _url;
		private readonly IJsonSerializer _serializer;
		private readonly HttpClient _httpClient;

		public HttpClientM(IHttpServer server, IMessageBrokerUrl url, IJsonSerializer serializer)
		{
			_server = server;
			_url = url;
			_serializer = serializer ?? new ToStringSerializer();
			_httpClient = new HttpClient(
				new HttpClientHandler
				{
					Credentials = CredentialCache.DefaultNetworkCredentials
				});
		}

		public void Post(string call, object thing)
		{
			var content = _serializer.SerializeObject(thing);
			var u = url(call);
			_server.Post(_httpClient, u, new StringContent(content, Encoding.UTF8, "application/json"));
		}

		public void PostOrThrow(string call, object thing)
		{
			var content = _serializer.SerializeObject(thing);
			var u = url(call);
			_server.PostOrThrow(_httpClient, u, new StringContent(content, Encoding.UTF8, "application/json"));
		}

		public string GetOrThrow(string call)
		{
			var u = url(call);
			return _server.GetOrThrow(_httpClient, u);
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