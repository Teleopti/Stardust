using System.Web;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.Web.Core.RequestContext;

namespace Teleopti.Ccc.WebTest
{
	public class FakeCurrentHttpContext : ICurrentHttpContext
	{
		private readonly HttpContextBase _httpContextBase;

		public FakeCurrentHttpContext(HttpContextBase httpContextBase) {
			_httpContextBase = httpContextBase;
		}

		public HttpContextBase Current() { return _httpContextBase; }
	}
}