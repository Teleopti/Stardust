using System.Collections.Generic;
using System.Net.Http;
using Teleopti.Messaging.Client.Http;

namespace Teleopti.MessagingTest
{
	public class FakeHttpServer : IHttpServer
	{
		public readonly IList<RequestInfo> Requests = new List<RequestInfo>();

		public void Post(HttpClient client, string uri, HttpContent httpContent)
		{
			Requests.Add(new RequestInfo {Client = client, Uri = uri, HttpContent = httpContent});
		}

		public void PostOrThrow(HttpClient client, string uri, HttpContent httpContent)
		{
			Requests.Add(new RequestInfo { Client = client, Uri = uri, HttpContent = httpContent });
		}

		public string GetOrThrow(HttpClient client, string uri)
		{
			Requests.Add(new RequestInfo {Client = client, Uri = uri});
			return null;
		}
	}
}