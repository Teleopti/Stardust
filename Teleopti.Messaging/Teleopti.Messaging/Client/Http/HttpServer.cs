using System.Net.Http;

namespace Teleopti.Messaging.Client.Http
{
	public class HttpServer : IHttpServer
	{
		public HttpResponseMessage PostAsync(HttpClient client, string uri, HttpContent httpContent)
		{
			return client.PostAsync(uri, httpContent).Result;
		}

		public HttpResponseMessage Get(HttpClient client, string uri)
		{
			return client.GetAsync(uri).Result;
		}
	}
}