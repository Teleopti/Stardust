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

		public UserController(ICurrentTeleoptiPrincipal currentTeleoptiPrincipal, IIanaTimeZoneProvider ianaTimeZoneProvider, ISessionSpecificWfmCookieProvider sessionWfmCookieProvider, IUserTimeZone userTimeZone)
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
			var regionnal = principalCacheable != null ? principalCacheable.Regional : principal.Regional;

			return new
			{
				UserName = principal.Identity.Name,
				DefaultTimeZone = _ianaTimeZoneProvider.WindowsToIana(defaultTimezone.Id),
				DefaultTimeZoneName = defaultTimezone.DisplayName,
				Language = regionnal.UICulture.IetfLanguageTag,
				DateFormatLocale = regionnal.Culture.Name,
				CultureInfo.CurrentCulture.NumberFormat,
				FirstDayOfWeek = (int)regionnal.Culture.DateTimeFormat.FirstDayOfWeek,
				regionnal.Culture.DateTimeFormat.DayNames,
				DateTimeFormat = new {
					regionnal.Culture.DateTimeFormat.ShortTimePattern,
					regionnal.Culture.DateTimeFormat.AMDesignator,
					regionnal.Culture.DateTimeFormat.PMDesignator
				},
				IsTeleoptiApplicationLogon = sessionData?.IsTeleoptiApplicationLogon ?? false
				
			};
		}
	}
}