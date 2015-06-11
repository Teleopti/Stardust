using System;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public class TenantAuthentication : ITenantAuthentication
	{
		private readonly ICurrentHttpContext _currentHttpContext;
		private readonly IFindPersonInfoByCredentials _findPersonByCredentials;
		private readonly ISessionSpecificDataProvider _sessionDataProvider;
		public static string PersonInfoKey = "personinfo";

		public TenantAuthentication(ICurrentHttpContext currentHttpContext, IFindPersonInfoByCredentials findPersonByCredentials, ISessionSpecificDataProvider sessionDataProvider)
		{
			_currentHttpContext = currentHttpContext;
			_findPersonByCredentials = findPersonByCredentials;
			_sessionDataProvider = sessionDataProvider;
		}

		public bool Logon()
		{
			var httpContext = _currentHttpContext.Current();
			PersonInfo tenantUser;
			var sessionData = _sessionDataProvider.GrabFromCookie();
			if (sessionData != null)
			{
				//web user
				tenantUser = _findPersonByCredentials.Find(sessionData.PersonId, sessionData.TenantPassword);
			}
			else
			{
				//external call
				var personId = httpContext.Request.Headers["personid"];
				var tenantpassword = httpContext.Request.Headers["tenantpassword"];
				tenantUser = _findPersonByCredentials.Find(Guid.Parse(personId), tenantpassword);
			}

			httpContext.Items[PersonInfoKey] = tenantUser;
			return tenantUser != null;
		}
	}
}