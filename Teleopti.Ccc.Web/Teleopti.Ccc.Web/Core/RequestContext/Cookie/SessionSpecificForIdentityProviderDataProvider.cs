using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Web;

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