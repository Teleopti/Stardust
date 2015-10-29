using System;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class SessionData : ISessionData
	{
		private TimeZoneInfo _timeZone;

		public SessionData()
		{
			_timeZone = TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone;
		}

		public TimeZoneInfo TimeZone
		{
			get { return _timeZone; }
			set
			{
				InParameter.NotNull("TimeZone", value);
				_timeZone = value;
			}
		}
	}
}
