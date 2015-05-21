using System;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.Web;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public class TenantAuthentication : ITenantAuthentication
	{
		private readonly ICurrentHttpContext _currentHttpContext;
		private readonly IFindPersonInfoByCredentials _findPersonByCredentials;
		public static string PersonInfoKey = "personinfo";

		public TenantAuthentication(ICurrentHttpContext currentHttpContext, IFindPersonInfoByCredentials findPersonByCredentials)
		{
			_currentHttpContext = currentHttpContext;
			_findPersonByCredentials = findPersonByCredentials;
		}

		public bool Logon()
		{
			var httpContext = _currentHttpContext.Current();
			var personId = httpContext.Request.Headers["personid"];
			var tenantpassword = httpContext.Request.Headers["tenantpassword"];
			var personInfo = _findPersonByCredentials.Find(Guid.Parse(personId), tenantpassword);
			if (personInfo == null)
				return false;
			
			httpContext.Items[PersonInfoKey] = personInfo;
			return true;
		}
	}
}