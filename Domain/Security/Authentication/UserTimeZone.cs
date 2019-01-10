using System;
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

		public TimeZoneInfo TimeZone() =>
			_loggedOnUser.Current()?.Regional.TimeZone;
	}
}