using System.Net.Http;

namespace Teleopti.Messaging.Client.Http
{
	public interface IHttpServer
	{
		void Post(HttpClient client, string uri, HttpContent httpContent);
		void PostOrThrow(HttpClient client, string uri, HttpContent httpContent);
		string GetOrThrow(HttpClient client, string uri);
	}
}