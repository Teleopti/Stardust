using System;
using System.Collections.Specialized;
using System.Web;
using Teleopti.Ccc.Infrastructure.Web;

namespace Teleopti.Ccc.TestCommon.Web
{
	public class FakeCurrentHttpContext : ICurrentHttpContext
	{
		private readonly HttpContextBase _httpContextBase;

		public FakeCurrentHttpContext()
		{
		//	Guid personId = Guid.NewGuid();
		//	string tenantPassword = "afsasdf";
			
		//	var headers = new NameValueCollection { { "personid", personId.ToString() }, { "tenantpassword", tenantPassword } };
			_httpContextBase = new FakeHttpContext(string.Empty, string.Empty, null, null, null, null, null, new NameValueCollection());
		}

		public FakeCurrentHttpContext(HttpContextBase httpContextBase) {
			_httpContextBase = httpContextBase;
		}

		public HttpContextBase Current() { return _httpContextBase; }

		
	}
}