using System.Net.Http;
using System.Threading.Tasks;

namespace Teleopti.Wfm.Api.Test
{
	public class ApiHttpClient : IApiHttpClient
	{
		private readonly HttpClient _client;

		public ApiHttpClient(HttpClient client)
		{
			_client = client;
		}

		public Task<HttpResponseMessage> GetAsync(string requestUri)
		{
			return _client.GetAsync(requestUri);
		}

		public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
		{
			return _client.PostAsync(requestUri, content);
		}

		public void Authorize()
		{
			_client.DefaultRequestHeaders.Add("Authorization", "bearer afdsafasdf");
		}
	}
}