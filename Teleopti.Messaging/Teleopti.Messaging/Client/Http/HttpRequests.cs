using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Interfaces;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Messaging.Client.Http
{
	public class HttpRequests
	{
		private readonly IMessageBrokerUrl _url;
		private readonly IJsonSerializer _serializer;

		public Func<HttpClient, string, HttpContent, Task<HttpResponseMessage>> PostAsync =
			(client, uri, httpContent) => client.PostAsync(uri, httpContent);

		public Func<HttpClient, string, Task<HttpResponseMessage>> GetAsync =
			(client, uri) => client.GetAsync(uri);

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

		public Task<HttpResponseMessage> Get(string call)
		{
			var u = url(call);
			return GetAsync(_httpClient, u);
		}

		public Task<HttpResponseMessage> Post(string call, object thing)
		{
			var content = _serializer.SerializeObject(thing);
			var u = url(call);
			return PostAsync(_httpClient, u, new StringContent(content, Encoding.UTF8, "application/json"));
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