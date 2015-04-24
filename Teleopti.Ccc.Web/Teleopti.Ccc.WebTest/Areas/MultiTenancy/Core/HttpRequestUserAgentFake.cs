using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy.Core
{
	public class HttpRequestUserAgentFake : IHttpRequestUserAgent
	{
		private readonly string _userAgent;

		public HttpRequestUserAgentFake(string userAgent)
		{
			_userAgent = userAgent;
		}

		public string Fetch()
		{
			return _userAgent;
		}
	}
}