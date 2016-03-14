using System.Collections.Generic;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeHttpServer : IHttpServer
	{
		public readonly IList<RequestInfo> Requests = new List<RequestInfo>();

		public void Post(string uri, object thing)
		{
			Requests.Add(new RequestInfo { Uri = uri, Thing = thing });
		}

		public void PostOrThrow(string uri, object thing)
		{
			Requests.Add(new RequestInfo { Uri = uri, Thing = thing });
		}

		public string GetOrThrow(string uri)
		{
			Requests.Add(new RequestInfo { Uri = uri });
			return null;
		}
	}
}