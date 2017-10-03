using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Web;

namespace Teleopti.Ccc.Web.Core.RequestContext.Cookie
{
	public class SessionSpecificTeleoptiCookieProvider : AbstractCookieDataProvider, ISessionSpecificTeleoptiCookieProvider
	{
		public SessionSpecificTeleoptiCookieProvider(ICurrentHttpContext httpContext,
			SessionSpecificCookieSettingsProvider sessionSpecificCookieSettingsProvider, INow now,
			ISessionSpecificDataStringSerializer dataStringSerializer)
			: base(httpContext, sessionSpecificCookieSettingsProvider.ForTeleopti(), now, dataStringSerializer
				)
		{
		}
	}
}