using System.Threading;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
    /// <summary>
    /// IState implemented class. 
    /// In normal cases use this instead of implementing IState
    /// and override the session behaviour.
    /// </summary>
    public abstract class State : IState
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
        /// Gets a value indicating whether the user is logged in or not.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if logged in; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsLoggedIn
        {
            get
            {
                var principal = Thread.CurrentPrincipal as ITeleoptiPrincipal;
                return (principal!=null && principal.Identity.IsAuthenticated);
            }
        }

        /// <summary>
        /// Gets the application scope data.
        /// </summary>
        /// <value>The application scope data.</value>
        public virtual IApplicationData ApplicationScopeData
        {
            get { return _applicationScopeData; }
        }


        /// <summary>
        /// Sets the session data.
        /// </summary>
        /// <param name="sessionData">The sess data.</param>
        public abstract void SetSessionData(ISessionData sessionData);

        /// <summary>
        /// Gets the session data for logged on user.
        /// </summary>
        /// <value>The session data.</value>
        public abstract ISessionData SessionScopeData { get; }


        /// <summary>
        /// Clears data kept for logged in session/user
        /// </summary>
        public abstract void ClearSession();
    }
}