using System;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class SessionData : ISessionData
	{
		public TimeZoneInfo TimeZone
		{
			get { return TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone; }
		}
	}
}
