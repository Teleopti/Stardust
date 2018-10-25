using System.Net.Http;
using System.Threading.Tasks;

namespace Teleopti.Ccc.Domain.Infrastructure
{
	public class HttpRequestHandler : IHttpRequestHandler
	{
		private readonly HttpClient _client = new HttpClient();

		public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
		{
			return await _client.SendAsync(request);
		}
	}
}