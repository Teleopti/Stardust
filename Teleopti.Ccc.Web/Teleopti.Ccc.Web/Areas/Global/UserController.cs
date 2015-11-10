using System.Globalization;
using System.Web.Http;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Filters;

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

		[Route("api/Global/User/CurrentUser"), HttpGet, AuthorizeTeleopti]
		public object CurrentUser()
		{
			var principal = _currentTeleoptiPrincipal.Current();
			return new
			{
				UserName = principal.Identity.Name,
				DefaultTimeZone = _ianaTimeZoneProvider.WindowsToIana(principal.Regional.TimeZone.Id),
				Language = CultureInfo.CurrentUICulture.IetfLanguageTag,
				DateFormatLocale = CultureInfo.CurrentCulture.Name,
				NumberFormat = CultureInfo.CurrentCulture.NumberFormat
			};
		}
	}
}