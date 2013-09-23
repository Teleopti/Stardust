using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Services
{
    /// <summary>
    /// Stores application data for the application lifetime  
    /// </summary>
    public class StateManager : State
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
        /// Gets the session data for logged on personItem.
        /// </summary>
        /// <value>The session data.</value>
        public override ISessionData SessionScopeData
        {
            get { return _sessData; }
        }

        /// <summary>
        /// Clears data kept for logged in session/personItem
        /// </summary>
        public override void ClearSession()
        {
            _sessData = null;
        }
    }
}
