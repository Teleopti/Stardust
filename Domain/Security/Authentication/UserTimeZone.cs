using System;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
	public class UserTimeZone : IUserTimeZone
	{
		private readonly ICurrentTeleoptiPrincipal _loggedOnUser;

		public UserTimeZone(ICurrentTeleoptiPrincipal loggedOnUser) 
		{
			_loggedOnUser = loggedOnUser;
		}

		public static IUserTimeZone Make()
		{
			return new UserTimeZone(new CurrentTeleoptiPrincipal(new ThreadPrincipalContext()));
		}

		public TimeZoneInfo TimeZone()     
		{
			var currentUser = _loggedOnUser.Current();
			if (currentUser == null) return null;

			var timezone = (currentUser is TeleoptiPrincipalCacheable principalCacheable) && (!principalCacheable.Person.PermissionInformation.DefaultTimeZoneString().IsNullOrEmpty())
				? principalCacheable.Person.PermissionInformation.DefaultTimeZone()
				: currentUser.Regional.TimeZone;


			return timezone;
		}
	}
}