using System;

namespace Teleopti.Ccc.Web.Core.RequestContext.Cookie
{
	public interface IBaseSessionSpecificCookieDataProviderSettings
	{
		TimeSpan AuthenticationCookieExpirationTimeSpan { get; }
		string AuthenticationCookieDomain { get; }
		bool AuthenticationCookieSlidingExpiration { get; }
		string AuthenticationCookiePath { get; }
		string AuthenticationCookieName { get; }
		bool AuthenticationRequireSsl { get; }
	}
}