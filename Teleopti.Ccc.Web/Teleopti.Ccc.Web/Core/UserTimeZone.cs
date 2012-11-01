using System;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core
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
            var dbg = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			return dbg;
		}
	}
}