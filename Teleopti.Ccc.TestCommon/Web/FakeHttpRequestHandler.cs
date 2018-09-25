using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Infrastructure;

namespace Teleopti.Ccc.TestCommon.Web
{
	public class FakeHttpRequestHandler : IHttpRequestHandler
	{
		private HttpResponseMessage _responseMessage;

		public HttpResponseMessage Get(string url)
		{
			throw new System.NotImplementedException();
		}

		public HttpResponseMessage Post(string url, HttpContent content)
		{
			throw new System.NotImplementedException();
		}

		public Task<HttpResponseMessage> GetAsync(string url)
		{
			throw new System.NotImplementedException();
		}

		public Task<HttpResponseMessage> PostAsync(string url, HttpContent content)
		{
			throw new System.NotImplementedException();
		}

		public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
		{
			return Task.FromResult(_responseMessage ?? new HttpResponseMessage(HttpStatusCode.OK));
		}

		public void SetFakeResponse(HttpResponseMessage responseMessage)
		{
			_responseMessage = responseMessage;
		}
	}
}