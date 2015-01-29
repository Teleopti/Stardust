using Teleopti.Ccc.Web.Areas.Tenant.Core;

namespace Teleopti.Ccc.WebTest.Areas.Tenant.Core
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