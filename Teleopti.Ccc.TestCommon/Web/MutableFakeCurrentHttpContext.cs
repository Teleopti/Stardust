using System;
using System.Web;
using Teleopti.Ccc.Infrastructure.Web;

namespace Teleopti.Ccc.TestCommon.Web
{
	public class MutableFakeCurrentHttpContext : ICurrentHttpContext
	{
		private HttpContextBase _httpContext;
		[ThreadStatic]
		private static HttpContextBase _httpContextOnThread;

		public void SetContext(HttpContextBase httpContext)
		{
			_httpContext = httpContext;
		}

		public void SetContextOnThread(HttpContextBase httpContext)
		{
			_httpContextOnThread = httpContext;
		}

		public HttpContextBase Current()
		{
			return _httpContextOnThread ?? _httpContext;
		}
	}
}