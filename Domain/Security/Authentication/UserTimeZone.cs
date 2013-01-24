using System;
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

		public TimeZoneInfo TimeZone()
		{
			var currentUser = _loggedOnUser.CurrentUser();
			return currentUser==null ? 
				null : 
				currentUser.PermissionInformation.DefaultTimeZone();
		}
	}
}