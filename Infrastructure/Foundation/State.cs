using System;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
    /// <summary>
    /// IState implemented class. 
    /// In normal cases use this instead of implementing IState
    /// and override the session behaviour.
    /// </summary>
    public class State : IState
    {
        private IApplicationData _applicationScopeData;

        /// <summary>
        /// Sets the application data.
        /// </summary>
        /// <param name="applicationData">The application data.</param>
        public virtual void SetApplicationData(IApplicationData applicationData)
        {
            _applicationScopeData = applicationData;
        }

        /// <summary>
        /// Gets the application scope data.
        /// </summary>
        /// <value>The application scope data.</value>
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