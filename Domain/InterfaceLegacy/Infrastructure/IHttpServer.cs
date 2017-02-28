using System;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Teleopti.Interfaces.Infrastructure
{
	public interface IHttpServer
	{
		Task Post(string uri, object thing, Func<string, NameValueCollection> customHeadersFunc = null);
		void PostOrThrow(string uri, object thing, Func<string, NameValueCollection> customHeadersFunc = null);
		Task PostOrThrowAsync(string uri, object thing, Func<string, NameValueCollection> customHeadersFunc = null);
		string GetOrThrow(string uri);
	}
}