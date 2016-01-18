using System;
using System.Web;
using System.Web.Security;
using Common.Logging;
using Microsoft.IdentityModel.Web;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core
{
	public class FormsAuthenticationWrapper : IFormsAuthentication
	{
		private readonly ILog _logger = LogManager.GetLogger<FormsAuthenticationWrapper>();
		private readonly ICurrentHttpContext _httpContext;
		private readonly ISessionSpecificForIdentityProviderDataProvider _sessionSpecificForIdentityProviderDataProvider;
		private readonly ISessionSpecificCookieForIdentityProviderDataProviderSettings _sessionSpecificCookieForIdentityProviderDataProviderSettings;
		private readonly INow _now;

		public FormsAuthenticationWrapper(ICurrentHttpContext httpContext, ISessionSpecificForIdentityProviderDataProvider sessionSpecificForIdentityProviderDataProvider, ISessionSpecificCookieForIdentityProviderDataProviderSettings sessionSpecificCookieForIdentityProviderDataProviderSettings, INow now)
		{
			_httpContext = httpContext;
			_sessionSpecificForIdentityProviderDataProvider = sessionSpecificForIdentityProviderDataProvider;
			_sessionSpecificCookieForIdentityProviderDataProviderSettings = sessionSpecificCookieForIdentityProviderDataProviderSettings;
			_now = now;
		}

		public void SetAuthCookie(string userName, bool isPersistent)
		{
			_sessionSpecificForIdentityProviderDataProvider.MakeCookie(userName, userName, isPersistent);
		}

		public void SignOut()
		{
			FederatedAuthentication.SessionAuthenticationModule.SignOut();
			FormsAuthentication.SignOut();

			var fedAuthCookie = new HttpCookie(_sessionSpecificCookieForIdentityProviderDataProviderSettings.AuthenticationCookieName) { Expires = _now.LocalDateTime().AddYears(-2) };
			_httpContext.Current().Response.Cookies.Remove(_sessionSpecificCookieForIdentityProviderDataProviderSettings.AuthenticationCookieName);
			_httpContext.Current().Response.Cookies.Add(fedAuthCookie);
		}

		public bool TryGetCurrentUser(out string userName)
		{
			HttpCookie authCookie = _httpContext.Current().Request.Cookies[_sessionSpecificCookieForIdentityProviderDataProviderSettings.AuthenticationCookieName];
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