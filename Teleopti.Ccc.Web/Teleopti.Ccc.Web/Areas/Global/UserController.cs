﻿using System;
using System.Globalization;
using System.Web.Http;
using Castle.Core.Internal;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class UserController : ApiController
	{
		private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;
		private readonly IIanaTimeZoneProvider _ianaTimeZoneProvider;
		private readonly ILoggedOnUser _loggedOnUser;

		public UserController(ICurrentTeleoptiPrincipal currentTeleoptiPrincipal, IIanaTimeZoneProvider ianaTimeZoneProvider, ILoggedOnUser loggedOnUser)
		{
			_currentTeleoptiPrincipal = currentTeleoptiPrincipal;
			_ianaTimeZoneProvider = ianaTimeZoneProvider;
			_loggedOnUser = loggedOnUser;
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

			var regionnal = principalCacheable != null ? principalCacheable.Regional : principal.Regional;
			return new
			{
				Id = _loggedOnUser.CurrentUser()?.Id ?? Guid.Empty,
				UserName = principal.Identity.Name,
				DefaultTimeZone = _ianaTimeZoneProvider.WindowsToIana(defaultTimezone.Id),
				DefaultTimeZoneName = defaultTimezone.DisplayName,
				Language = regionnal.UICulture.IetfLanguageTag,
				DateFormatLocale = regionnal.Culture.Name,
				CultureInfo.CurrentCulture.NumberFormat,
				FirstDayOfWeek = (int)regionnal.Culture.DateTimeFormat.FirstDayOfWeek
			};
		}
	}
}