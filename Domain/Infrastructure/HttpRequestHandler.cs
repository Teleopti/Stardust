using System.Net.Http;
using System.Threading.Tasks;

namespace Teleopti.Ccc.Domain.Infrastructure
{
	public class HttpRequestHandler : IHttpRequestHandler
	{
		private readonly HttpClient _client = new HttpClient();

		public HttpResponseMessage Get(string url)
		{
			return GetAsync(url).Result;
		}

		public HttpResponseMessage Post(string url, HttpContent content)
		{
			return PostAsync(url, content).Result;
		}

		public async Task<HttpResponseMessage> GetAsync(string url)
		{
			return await _client.GetAsync(url);
		}

		public async Task<HttpResponseMessage> PostAsync(string url, HttpContent content)
		{
			return await _client.PostAsync(url, content);
		}

		public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
		{
			return await _client.SendAsync(request);
		}
	}
}