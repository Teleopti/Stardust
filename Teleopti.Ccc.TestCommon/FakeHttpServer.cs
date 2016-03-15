using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeHttpServer : IHttpServer
	{
		public readonly IList<RequestInfo> Requests = new List<RequestInfo>();

		public void Post(string uri, object thing, Func<string, NameValueCollection> customHeadersFunc = null)
		{
			var headers = customHeadersFunc != null ? customHeadersFunc(thing.ToString()) : null;
			Requests.Add(new RequestInfo { Uri = uri, Thing = thing, Headers = headers});
		}

		public void PostOrThrow(string uri, object thing, Func<string, NameValueCollection> customHeadersFunc = null)
		{
			var headers = customHeadersFunc != null ? customHeadersFunc(thing.ToString()) : null;
			Requests.Add(new RequestInfo { Uri = uri, Thing = thing, Headers = headers});
		}

		public string GetOrThrow(string uri)
		{
			Requests.Add(new RequestInfo { Uri = uri });
			return null;
		}
	}
}