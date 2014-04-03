using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.RequestContext.Cookie
{
	public class SessionSpecificForIdentityProviderDataProvider : AbstractCookieDataProvider, ISessionSpecificForIdentityProviderDataProvider
	{
		public SessionSpecificForIdentityProviderDataProvider(ICurrentHttpContext httpContext,
			ISessionSpecificCookieForIdentityProviderDataProviderSettings sessionSpecificCookieDataProviderSettings, INow now,
			ISessionSpecificDataStringSerializer dataStringSerializer, ISessionAuthenticationModule sessionAuthenticationModule)
			: base(httpContext, sessionSpecificCookieDataProviderSettings, now, dataStringSerializer, sessionAuthenticationModule
				)
		{
		}
	}
}