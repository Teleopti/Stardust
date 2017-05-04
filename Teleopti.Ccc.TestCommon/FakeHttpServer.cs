using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeHttpServer : IHttpServer
	{
		public readonly IList<RequestInfo> Requests = new List<RequestInfo>();
		private HttpResponseMessage _responseMessage;

		private Task<HttpResponseMessage> _postRespTask;

		public Task<HttpResponseMessage> Post(string uri, object thing, Func<string, NameValueCollection> customHeadersFunc = null)
		{
			var headers = customHeadersFunc != null ? customHeadersFunc(thing.ToString()) : null;
			Requests.Add(new RequestInfo { Uri = uri, Thing = thing, Headers = headers });
			_postRespTask = Task.FromResult(_responseMessage ?? new HttpResponseMessage(HttpStatusCode.OK));
			return _postRespTask;
		}

		public void PostOrThrow(string uri, object thing, Func<string, NameValueCollection> customHeadersFunc = null)
		{
			var headers = customHeadersFunc != null ? customHeadersFunc(thing.ToString()) : null;
			Requests.Add(new RequestInfo { Uri = uri, Thing = thing, Headers = headers });
		}

		public Task PostOrThrowAsync(string uri, object thing, Func<string, NameValueCollection> customHeadersFunc = null)
		{
			var headers = customHeadersFunc != null ? customHeadersFunc(thing.ToString()) : null;
			Requests.Add(new RequestInfo { Uri = uri, Thing = thing, Headers = headers });
			return Task.FromResult(false);
		}

		public void FakeResponseMessage(HttpResponseMessage responseMessage)
		{
			this._responseMessage = responseMessage;
		}

		public string GetOrThrow(string uri)
		{
			Requests.Add(new RequestInfo { Uri = uri });
			return null;
		}

		public Task<HttpResponseMessage> LastPostResponseTask  => _postRespTask; 
	}
}