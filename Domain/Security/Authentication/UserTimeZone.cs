using System;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
	public class UserTimeZone : IUserTimeZone
	{
		private readonly ILoggedOnUser _loggedOnUser;

		public UserTimeZone(ILoggedOnUser loggedOnUser) 
		{
			_loggedOnUser = loggedOnUser;
		}

		public static IUserTimeZone Make()
		{
			return new UserTimeZone(new LoggedOnUser(null, new CurrentTeleoptiPrincipal()));
		}

		public TimeZoneInfo TimeZone()     
		{
			var currentUser = _loggedOnUser.CurrentUser();
			return currentUser==null ? 
				null : 
				currentUser.PermissionInformation.DefaultTimeZone();
		}
	}
}