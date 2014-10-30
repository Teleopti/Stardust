using System.Web;
using Teleopti.Ccc.Infrastructure.Web;

namespace Teleopti.Ccc.TestCommon.Web
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