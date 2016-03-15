using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeHttpServer : IHttpServer
	{
		public readonly IList<RequestInfo> Requests = new List<RequestInfo>();

		public Task Post(string uri, object thing, Func<string, NameValueCollection> customHeadersFunc = null)
		{
			var headers = customHeadersFunc != null ? customHeadersFunc(thing.ToString()) : null;
			Requests.Add(new RequestInfo { Uri = uri, Thing = thing, Headers = headers});
			return Task.FromResult(false);
		}

		public void PostOrThrow(string uri, object thing, Func<string, NameValueCollection> customHeadersFunc = null)
		{
			var headers = customHeadersFunc != null ? customHeadersFunc(thing.ToString()) : null;
			Requests.Add(new RequestInfo { Uri = uri, Thing = thing, Headers = headers});
		}

		public Task PostOrThrowAsync(string uri, object thing, Func<string, NameValueCollection> customHeadersFunc = null)
		{
			var headers = customHeadersFunc != null ? customHeadersFunc(thing.ToString()) : null;
			Requests.Add(new RequestInfo { Uri = uri, Thing = thing, Headers = headers});
			return Task.FromResult(false);
		}

		public string GetOrThrow(string uri)
		{
			Requests.Add(new RequestInfo { Uri = uri });
			return null;
		}
	}
}