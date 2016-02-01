using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.RequestContext.Cookie
{
	public class SessionSpecificForIdentityProviderDataProvider : AbstractCookieDataProvider, ISessionSpecificForIdentityProviderDataProvider
	{
		public SessionSpecificForIdentityProviderDataProvider(ICurrentHttpContext httpContext,
			ISessionSpecificCookieForIdentityProviderDataProviderSettings sessionSpecificCookieDataProviderSettings, INow now,
			ISessionSpecificDataStringSerializer dataStringSerializer)
			: base(httpContext, sessionSpecificCookieDataProviderSettings, now, dataStringSerializer
				)
		{
		}
	}
}