using System.Net;
using System.Net.Http;
using System.Text;
using Teleopti.Interfaces;

namespace Teleopti.Messaging.Client.Http
{
	public class HttpServer : IHttpServer
	{
		private readonly HttpClient _client;
		private readonly IJsonSerializer _serializer;

		public HttpServer(IJsonSerializer serializer) : this(null, serializer)
		{
		}

		public HttpServer(HttpClient client, IJsonSerializer serializer)
		{
			_client = client ?? new HttpClient(new HttpClientHandler {Credentials = CredentialCache.DefaultNetworkCredentials});
			_serializer = serializer ?? new ToStringSerializer();
		}

		public void Post(string uri, object thing)
		{
			var content = _serializer.SerializeObject(thing);
			_client.PostAsync(uri, new StringContent(content, Encoding.UTF8, "application/json"));
		}

		public void PostOrThrow(string uri, object thing)
		{
			var content = _serializer.SerializeObject(thing);
			_client
				.PostAsync(uri, new StringContent(content, Encoding.UTF8, "application/json"))
				.Result
				.EnsureSuccessStatusCode();
		}

		public string GetOrThrow(string uri)
		{
			return _client
				.GetAsync(uri)
				.Result
				.EnsureSuccessStatusCode()
				.Content
				.ReadAsStringAsync()
				.Result;
		}
	}
}