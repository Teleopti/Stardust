using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Web;

namespace Teleopti.Ccc.Web.Core.RequestContext.Cookie
{
	public class SessionSpecificCookieDataProvider : AbstractCookieDataProvider, ISessionSpecificDataProvider
	{
		public SessionSpecificCookieDataProvider(ICurrentHttpContext httpContext,
			ISessionSpecificCookieDataProviderSettings sessionSpecificCookieDataProviderSettings, INow now,
			ISessionSpecificDataStringSerializer dataStringSerializer)
			: base(httpContext, sessionSpecificCookieDataProviderSettings, now, dataStringSerializer
				)
		{
		}
	}
}