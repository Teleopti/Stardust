using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;

namespace Teleopti.Ccc.Web.Core.RequestContext.Cookie
{
	public class SessionSpecificWfmCookieProvider : AbstractCookieDataProvider, ISessionSpecificWfmCookieProvider
	{
		public SessionSpecificWfmCookieProvider(ICurrentHttpContext httpContext,
			SessionSpecificCookieSettingsProvider sessionSpecificCookieSettingsProvider, INow now,
			ISessionSpecificDataStringSerializer dataStringSerializer,
			MaximumSessionTimeProvider maximumSessionTimeProvider)
			: base(
				httpContext, sessionSpecificCookieSettingsProvider.ForWfm(), now, dataStringSerializer, maximumSessionTimeProvider
			)
		{
		}
	}
}