using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;

namespace Teleopti.Ccc.Web.Core.RequestContext.Cookie
{
	public class SessionSpecificTeleoptiCookieProvider : AbstractCookieDataProvider, ISessionSpecificTeleoptiCookieProvider
	{
		public SessionSpecificTeleoptiCookieProvider(ICurrentHttpContext httpContext,
			SessionSpecificCookieSettingsProvider sessionSpecificCookieSettingsProvider,
			INow now,
			ISessionSpecificDataStringSerializer dataStringSerializer,
			MaximumSessionTimeProvider maximumSessionTimeProvider)
			: base(
				httpContext, sessionSpecificCookieSettingsProvider.ForTeleopti(), now, dataStringSerializer,
				maximumSessionTimeProvider
			)
		{
		}
	}
}