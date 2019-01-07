using System;
using System.Web;
using System.Web.Security;
using Common.Logging;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;

namespace Teleopti.Ccc.Web.Core
{
	public class FormsAuthenticationWrapper : IFormsAuthentication
	{
		private readonly ILog _logger = LogManager.GetLogger<FormsAuthenticationWrapper>();
		private readonly ICurrentHttpContext _httpContext;
		private readonly ISessionSpecificTeleoptiCookieProvider _sessionSpecificTeleoptiCookieProvider;
		private readonly ISessionSpecificCookieSettings _sessionSpecificCookieSettingsForTeleoptiIdentityProvider;
		private readonly INow _now;

		public FormsAuthenticationWrapper(ICurrentHttpContext httpContext, ISessionSpecificTeleoptiCookieProvider sessionSpecificTeleoptiCookieProvider, INow now,  SessionSpecificCookieSettingsProvider sessionSpecificCookieSettingsProvider)
		{
			_httpContext = httpContext;
			_sessionSpecificTeleoptiCookieProvider = sessionSpecificTeleoptiCookieProvider;
			_sessionSpecificCookieSettingsForTeleoptiIdentityProvider = sessionSpecificCookieSettingsProvider.ForTeleopti();
			_now = now;
		}

		public void SetAuthCookie(string userName, bool isPersistent, bool isLogonFromBrowser, string tenantName)
		{
			_sessionSpecificTeleoptiCookieProvider.MakeCookie(userName, userName, isPersistent, isLogonFromBrowser, tenantName);
		}

		public void SignOut()
		{
			FormsAuthentication.SignOut();
			var fedAuthCookie =
				new HttpCookie(_sessionSpecificCookieSettingsForTeleoptiIdentityProvider.AuthenticationCookieName)
				{
					Expires = _now.ServerDateTime_DontUse().AddYears(-2), HttpOnly = true,
					Secure = _sessionSpecificCookieSettingsForTeleoptiIdentityProvider.AuthenticationRequireSsl
				};
			var httpCookieCollection = _httpContext.Current().Response.Cookies;
			httpCookieCollection.Remove(_sessionSpecificCookieSettingsForTeleoptiIdentityProvider.AuthenticationCookieName);
			httpCookieCollection.Add(fedAuthCookie);
		}

		public bool TryGetCurrentUser(out string userName)
		{
			HttpCookie authCookie = _httpContext.Current().Request.Cookies[_sessionSpecificCookieSettingsForTeleoptiIdentityProvider.AuthenticationCookieName];
			if (authCookie != null)
			{
				try
				{
					FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);
					if (ticket != null && !ticket.Expired)
					{
						userName = ticket.Name;
						_logger.DebugFormat("Found user {0}!",userName);
						return true;
					}
				}
				catch (Exception)
				{
				}
			}

			_logger.Debug("Didn't find user!");
			userName = null;
			return false;
		}
	}
}