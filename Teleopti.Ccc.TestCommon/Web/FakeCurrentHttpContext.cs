using System;
using System.Collections.Specialized;
using System.Web;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.Web;

namespace Teleopti.Ccc.TestCommon.Web
{
	public class FakeCurrentHttpContext : ICurrentHttpContext
	{
		private readonly HttpContextBase _httpContextBase;

		public FakeCurrentHttpContext()
		{
			var fakeHttpContext = new FakeHttpContext(string.Empty, string.Empty, null, null, null, null, null, new NameValueCollection());
			var pi = new PersonInfo(new Tenant("_"), Guid.NewGuid());
			fakeHttpContext.AddItem(WebTenantAuthenticationConfiguration.PersonInfoKey, pi);
			_httpContextBase = fakeHttpContext;
		}

		public FakeCurrentHttpContext(HttpContextBase httpContextBase) {
			_httpContextBase = httpContextBase;
		}

		public HttpContextBase Current() { return _httpContextBase; }

		
	}
}