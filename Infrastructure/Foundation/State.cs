using System;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
    public class State : IState
    {
        private IApplicationData _applicationScopeData;

	    public virtual void SetApplicationData(IApplicationData applicationData)
		{
			_applicationScopeData = applicationData;
		}

		public virtual IApplicationData ApplicationScopeData
        {
            get { return _applicationScopeData; }
        }

		public TimeZoneInfo UserTimeZone
		{
			get { return TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone; }
		}
	}
}