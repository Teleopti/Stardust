using System.Net.Http;
using System.Threading.Tasks;

namespace Teleopti.Messaging.Client.Http
{
	public interface IHttpServer
	{
		void PostAsync(HttpClient client, string uri, HttpContent httpContent);
		Task<HttpResponseMessage> GetAsync(HttpClient client, string uri);
	}

	public class HttpServer : IHttpServer
	{
		public void PostAsync(HttpClient client, string uri, HttpContent httpContent)
		{
			client.PostAsync(uri, httpContent);
		}

		public Task<HttpResponseMessage> GetAsync(HttpClient client, string uri)
		{
			return client.GetAsync(uri);
		}
	}
}