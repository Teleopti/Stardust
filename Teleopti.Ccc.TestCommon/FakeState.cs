using System;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeState : IState
	{
		public bool IsLoggedIn { get; set; }
		public ISessionData SessionScopeData { get; set; }
		public TimeZoneInfo UserTimeZone { get { return TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone; } }
		public IApplicationData ApplicationScopeData { get; set; }

		public void ClearSession()
		{
			SessionScopeData = null;
		}

		public void SetSessionData(ISessionData sessionData)
		{
			SessionScopeData = sessionData;
		}

		public void SetApplicationData(IApplicationData applicationData)
		{
			ApplicationScopeData = applicationData;
		}
	}
}