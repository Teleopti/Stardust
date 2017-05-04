using System;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{
	public interface IHttpServer
	{
		Task<HttpResponseMessage> Post(string uri, object thing, Func<string, NameValueCollection> customHeadersFunc = null);
		void PostOrThrow(string uri, object thing, Func<string, NameValueCollection> customHeadersFunc = null);
		Task PostOrThrowAsync(string uri, object thing, Func<string, NameValueCollection> customHeadersFunc = null);
		string GetOrThrow(string uri);
	}
}