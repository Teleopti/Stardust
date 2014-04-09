using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.RequestContext.Cookie
{
	public class SessionSpecificCookieDataProvider : AbstractCookieDataProvider, ISessionSpecificDataProvider
	{
		public SessionSpecificCookieDataProvider(ICurrentHttpContext httpContext,
			ISessionSpecificCookieDataProviderSettings sessionSpecificCookieDataProviderSettings, INow now,
			ISessionSpecificDataStringSerializer dataStringSerializer, ISessionAuthenticationModule sessionAuthenticationModule)
			: base(httpContext, sessionSpecificCookieDataProviderSettings, now, dataStringSerializer, sessionAuthenticationModule
				)
		{
		}
	}
}