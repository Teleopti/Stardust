using System;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	/// <summary>
	/// Holds applicationspecific settings
	/// </summary>
	public interface ISessionSpecificCookieDataProviderSettings
	{
		TimeSpan AuthenticationCookieExpirationTimeSpan { get; }
		string AuthenticationCookieDomain { get; }
		bool AuthenticationCookieSlidingExpiration { get; }
		string AuthenticationCookiePath { get; }
		string AuthenticationCookieName { get; }
		bool AuthenticationRequireSsl { get; }
	}
}