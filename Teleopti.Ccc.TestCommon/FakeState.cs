using System;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeState : IState
	{
		public TimeZoneInfo UserTimeZone { get { return TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone; } }
		public IApplicationData ApplicationScopeData { get; set; }

		public void SetApplicationData(IApplicationData applicationData)
		{
			ApplicationScopeData = applicationData;
		}
	}
}