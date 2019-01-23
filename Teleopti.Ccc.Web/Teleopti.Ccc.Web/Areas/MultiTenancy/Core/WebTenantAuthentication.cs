using System;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public class WebTenantAuthentication : ITenantAuthentication
	{
		private readonly ICurrentHttpContext _currentHttpContext;
		private readonly IFindPersonInfoByCredentials _findPersonByCredentials;
		private readonly ISessionSpecificWfmCookieProvider _sessionWfmCookieProvider;

		public WebTenantAuthentication(ICurrentHttpContext currentHttpContext, IFindPersonInfoByCredentials findPersonByCredentials, ISessionSpecificWfmCookieProvider sessionWfmCookieProvider)
		{
			_currentHttpContext = currentHttpContext;
			_findPersonByCredentials = findPersonByCredentials;
			_sessionWfmCookieProvider = sessionWfmCookieProvider;
		}

		public bool Logon()
		{
			var httpContext = _currentHttpContext.Current();
			PersonInfo tenantUser;
			var sessionData = _sessionWfmCookieProvider.GrabFromCookie();
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

			httpContext.Items[WebTenantAuthenticationConfiguration.PersonInfoKey] = tenantUser;
			return tenantUser != null;
		}
	}
}