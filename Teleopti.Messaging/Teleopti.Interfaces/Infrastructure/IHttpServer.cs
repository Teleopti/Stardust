using System;
using System.Collections.Specialized;

namespace Teleopti.Interfaces.Infrastructure
{
	public interface IHttpServer
	{
		void Post(string uri, object thing, Func<string, NameValueCollection> customHeadersFunc = null);
		void PostOrThrow(string uri, object thing, Func<string, NameValueCollection> customHeadersFunc = null);
		string GetOrThrow(string uri);
	}
}