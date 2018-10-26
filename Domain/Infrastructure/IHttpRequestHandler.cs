using System.Net.Http;
using System.Threading.Tasks;

namespace Teleopti.Ccc.Domain.Infrastructure
{
	public interface IHttpRequestHandler
	{
		Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
	}
}
