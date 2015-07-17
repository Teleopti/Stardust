using System.Net.Http;

namespace Teleopti.Messaging.Client.Http
{
	public interface IHttpServer
	{
		HttpResponseMessage PostAsync(HttpClient client, string uri, HttpContent httpContent);
		HttpResponseMessage Get(HttpClient client, string uri);
	}
}