using System.Globalization;
using System.Web.Http;
using Castle.Core.Internal;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class UserController : ApiController
	{
		private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;
		private readonly IIanaTimeZoneProvider _ianaTimeZoneProvider;

		public UserController(ICurrentTeleoptiPrincipal currentTeleoptiPrincipal, IIanaTimeZoneProvider ianaTimeZoneProvider)
		{
			_currentTeleoptiPrincipal = currentTeleoptiPrincipal;
			_ianaTimeZoneProvider = ianaTimeZoneProvider;
		}

		[Route("api/Global/User/CurrentUser"), HttpGet]
		public object CurrentUser()
		{
			var principal = _currentTeleoptiPrincipal.Current();
			var principalCacheable = principal as TeleoptiPrincipalCacheable;
			
			var defaultTimezone = principalCacheable != null && !principalCacheable.Person.PermissionInformation.DefaultTimeZoneString().IsNullOrEmpty()
				? principalCacheable.Person.PermissionInformation.DefaultTimeZone()
				: principal.Regional.TimeZone;
			
			var regionnal = principalCacheable != null ? principalCacheable.Regional: principal.Regional;
			return new
			{
				UserName = principal.Identity.Name,
				DefaultTimeZone = _ianaTimeZoneProvider.WindowsToIana(defaultTimezone.Id),
				DefaultTimeZoneName = defaultTimezone.DisplayName,
				Language = regionnal.UICulture.IetfLanguageTag,
				DateFormatLocale = regionnal.Culture.Name,
				CultureInfo.CurrentCulture.NumberFormat
			};
		}
	}
}