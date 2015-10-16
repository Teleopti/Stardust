using System.Net.Http;

namespace Teleopti.Messaging.Client.Http
{
	public class HttpServer : IHttpServer
	{
		public void Post(HttpClient client, string uri, HttpContent httpContent)
		{
			client.PostAsync(uri, httpContent);
		}

		public void PostOrThrow(HttpClient client, string uri, HttpContent httpContent)
		{
			client
				.PostAsync(uri, httpContent)
				.Result
				.EnsureSuccessStatusCode();
		}

		public string GetOrThrow(HttpClient client, string uri)
		{
			return client
				.GetAsync(uri)
				.Result
				.EnsureSuccessStatusCode()
				.Content
				.ReadAsStringAsync()
				.Result;
		}
	}
}