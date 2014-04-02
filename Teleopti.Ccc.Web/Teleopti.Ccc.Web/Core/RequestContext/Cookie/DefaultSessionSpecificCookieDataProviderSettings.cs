using System;
using System.Web.Security;

namespace Teleopti.Ccc.Web.Core.RequestContext.Cookie
{
	public class DefaultSessionSpecificCookieDataProviderSettings : ISessionSpecificCookieDataProviderSettings
	{
		public DefaultSessionSpecificCookieDataProviderSettings()
		{
			// Move these to appSettings or create own configsection
			AuthenticationCookieDomain = FormsAuthentication.CookieDomain;
			AuthenticationCookieExpirationTimeSpan = new TimeSpan(0, 30, 0);
			AuthenticationCookieName = "TeleoptiAuth";
			AuthenticationCookiePath = FormsAuthentication.FormsCookiePath;
			AuthenticationCookieSlidingExpiration = FormsAuthentication.SlidingExpiration;
			AuthenticationRequireSsl = FormsAuthentication.RequireSSL;
		}

		public string AuthenticationCookieDomain { get; private set; }

		public TimeSpan AuthenticationCookieExpirationTimeSpan { get; private set; }

		public bool AuthenticationCookieSlidingExpiration { get; private set; }

		public string AuthenticationCookiePath { get; private set; }

		public string AuthenticationCookieName { get; private set; }

		public bool AuthenticationRequireSsl { get; private set; }
	}
}