using System.Net.Http;
using System.Threading.Tasks;

namespace Teleopti.Messaging.Client.Http
{
	public interface IHttpServer
	{
		Task<HttpResponseMessage> PostAsync(HttpClient client, string uri, HttpContent httpContent);
		Task<HttpResponseMessage> GetAsync(HttpClient client, string uri);
	}
}