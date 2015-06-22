using System.Net.Http;

namespace Teleopti.Messaging.Client.Http
{
	public interface IHttpServer
	{
		void PostAsync(HttpClient client, string uri, HttpContent httpContent);
		string Get(HttpClient client, string uri);
	}

	public class HttpServer : IHttpServer
	{
		public void PostAsync(HttpClient client, string uri, HttpContent httpContent)
		{
			client.PostAsync(uri, httpContent);
		}

		public string Get(HttpClient client, string uri)
		{
			return client
				.GetAsync(uri).Result
				.Content.ReadAsStringAsync().Result;
		}
	}
}