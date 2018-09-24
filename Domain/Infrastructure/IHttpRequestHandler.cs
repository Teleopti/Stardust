using System.Net.Http;
using System.Threading.Tasks;

namespace Teleopti.Ccc.Domain.Infrastructure
{
	public interface IHttpRequestHandler
	{
		HttpResponseMessage Get(string url);
		HttpResponseMessage Post(string url, HttpContent content);
		Task<HttpResponseMessage> GetAsync(string url);
		Task<HttpResponseMessage> PostAsync(string url, HttpContent content);
		Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
	}
}
