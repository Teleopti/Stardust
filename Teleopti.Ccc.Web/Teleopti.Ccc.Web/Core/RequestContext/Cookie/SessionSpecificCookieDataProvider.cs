using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Interfaces.Domain;

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