using System;
using System.Web.Security;

namespace Teleopti.Ccc.Web.Core.RequestContext.Cookie
{
	public class DefaultSessionSpecificCookieForIdentityProviderDataProviderSettings : ISessionSpecificCookieSettings
	{
		public DefaultSessionSpecificCookieForIdentityProviderDataProviderSettings()
		{
			// Move these to appSettings or create own configsection
			AuthenticationCookieDomain = FormsAuthentication.CookieDomain;
			AuthenticationCookieExpirationTimeSpan = new TimeSpan(0, 30, 0);
			AuthenticationCookieExpirationTimeSpanLong = new TimeSpan(30, 0, 0, 0);
			AuthenticationCookieName = "TeleoptiIpAuth";
			AuthenticationCookiePath = FormsAuthentication.FormsCookiePath;
			AuthenticationCookieSlidingExpiration = FormsAuthentication.SlidingExpiration;
			AuthenticationRequireSsl = FormsAuthentication.RequireSSL;
		}

		public string AuthenticationCookieDomain { get; }
		public TimeSpan AuthenticationCookieExpirationTimeSpan { get; }
		public TimeSpan AuthenticationCookieExpirationTimeSpanLong { get; }
		public bool AuthenticationCookieSlidingExpiration { get; }
		public string AuthenticationCookiePath { get; }
		public string AuthenticationCookieName { get; }
		public bool AuthenticationRequireSsl { get; }
	}
}