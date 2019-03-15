using Castle.Core.Internal;
using System.Globalization;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class UserController : ApiController
	{
		private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;
		private readonly IIanaTimeZoneProvider _ianaTimeZoneProvider;
		private readonly ISessionSpecificWfmCookieProvider _sessionWfmCookieProvider;
		private readonly IUserTimeZone _userTimeZone;

		public UserController(ICurrentTeleoptiPrincipal currentTeleoptiPrincipal,
			IIanaTimeZoneProvider ianaTimeZoneProvider,
			ISessionSpecificWfmCookieProvider sessionWfmCookieProvider,
			IUserTimeZone userTimeZone)
		{
			_currentTeleoptiPrincipal = currentTeleoptiPrincipal;
			_ianaTimeZoneProvider = ianaTimeZoneProvider;
			_sessionWfmCookieProvider = sessionWfmCookieProvider;
			_userTimeZone = userTimeZone;
		}

		[Route("api/Global/User/CurrentUser"), HttpGet]
		[UnitOfWork]
		public virtual object CurrentUser()
		{
			var principal = _currentTeleoptiPrincipal.Current();
			var principalCacheable = principal as TeleoptiPrincipal;

			var defaultTimezone = _userTimeZone.TimeZone();

			var sessionData = _sessionWfmCookieProvider.GrabFromCookie();
			var regional = principalCacheable != null ? principalCacheable.Regional : principal.Regional;

			return new
			{
				UserName = principal.Identity.Name,
				DefaultTimeZone = _ianaTimeZoneProvider.WindowsToIana(defaultTimezone.Id),
				DefaultTimeZoneName = defaultTimezone.DisplayName,
				Language = regional.UICulture.IetfLanguageTag,
				DateFormatLocale = regional.Culture.Name,
				CultureInfo.CurrentCulture.NumberFormat,
				FirstDayOfWeek = (int)regional.Culture.DateTimeFormat.FirstDayOfWeek,
				regional.Culture.DateTimeFormat.DayNames,
				DateTimeFormat = new {
					regional.Culture.DateTimeFormat.ShortTimePattern,
					regional.Culture.DateTimeFormat.AMDesignator,
					regional.Culture.DateTimeFormat.PMDesignator
				},
				IsTeleoptiApplicationLogon = sessionData?.IsTeleoptiApplicationLogon ?? false
			};
		}
	}
}