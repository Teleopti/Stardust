using Castle.Core.Internal;
using System;
using System.Globalization;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.Extensions;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class UserController : ApiController
	{
		private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;
		private readonly IIanaTimeZoneProvider _ianaTimeZoneProvider;
		private readonly ISessionSpecificWfmCookieProvider _sessionWfmCookieProvider;

		public UserController(ICurrentTeleoptiPrincipal currentTeleoptiPrincipal, IIanaTimeZoneProvider ianaTimeZoneProvider, ISessionSpecificWfmCookieProvider sessionWfmCookieProvider)
		{
			_currentTeleoptiPrincipal = currentTeleoptiPrincipal;
			_ianaTimeZoneProvider = ianaTimeZoneProvider;
			_sessionWfmCookieProvider = sessionWfmCookieProvider;
		}

		[Route("api/Global/User/CurrentUser"), HttpGet]
		[UnitOfWork]
		public virtual object CurrentUser()
		{
			var principal = _currentTeleoptiPrincipal.Current();
			var principalCacheable = principal as TeleoptiPrincipalCacheable;

			var defaultTimezone = principalCacheable != null && !principalCacheable.Person.PermissionInformation.DefaultTimeZoneString().IsNullOrEmpty()
				? principalCacheable.Person.PermissionInformation.DefaultTimeZone()
				: principal.Regional.TimeZone;

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
				IsTeleoptiApplicationLogon = sessionData?.IsTeleoptiApplicationLogon ?? false,
				NowInUtc = DateTime.UtcNow.ToServiceDateFormat()
			};
		}
	}
}