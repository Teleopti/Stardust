using System;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

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
			return new UserTimeZone(new CurrentTeleoptiPrincipal());
		}

		public TimeZoneInfo TimeZone()     
		{
			var currentUser = _loggedOnUser.Current();
			if (currentUser == null) return null;

			return currentUser.Regional.TimeZone;
		}
	}
}