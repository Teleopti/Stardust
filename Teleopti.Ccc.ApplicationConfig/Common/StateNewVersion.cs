using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.ApplicationConfig.Common
{
    public class StateNewVersion : State
    {
        private ISessionData _sessData;

        /// <summary>
        /// Sets the session data.
        /// </summary>
        /// <param name="sessionData">The sess data.</param>
        public override void SetSessionData(ISessionData sessionData)
        {
            _sessData = sessionData;
        }

        /// <summary>
        /// Gets the session data for logged on user.
        /// </summary>
        /// <value>The session data.</value>
        public override ISessionData SessionScopeData
        {
            get { return _sessData; }
        }

        /// <summary>
        /// Clears data kept for logged in session/user
        /// </summary>
        public override void ClearSession()
        {
            _sessData = null;
        }
    }
}