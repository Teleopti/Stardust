using System;
using System.Net;
using System.Net.Http;
using System.Text;
using Teleopti.Interfaces;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Messaging.Client.Http
{
	public class HttpRequests
	{
		private readonly IMessageBrokerUrl _url;
		private readonly IJsonSerializer _serializer;

		public Action<HttpClient, string, HttpContent> PostAsync =
			(client, uri, httpContent) => client.PostAsync(uri, httpContent);

		private readonly HttpClient _httpClient;

		public HttpRequests(IMessageBrokerUrl url, IJsonSerializer serializer)
		{
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