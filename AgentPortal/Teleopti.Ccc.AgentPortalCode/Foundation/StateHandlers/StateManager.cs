namespace Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers
{
   /// <summary>
    /// State Manager for Holding session
    /// </summary>
    /// <remarks>
    /// Created by: Dinesh Ranasinghe
    /// Created date: 2008-03-04
    /// </remarks>
   public class StateManager:State
    {
        private ISessionData _sessionData;

        /// <summary>
        /// Sets the session data.
        /// </summary>
        /// <param name="sessionData">The session data.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-03-04
        /// </remarks>
        public override void SetSessionData(ISessionData sessionData)
        {
            _sessionData = sessionData;    
        }

        /// <summary>
        /// Gets the session scope data.
        /// </summary>
        /// <value>The session scope data.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-03-04
        /// </remarks>
        public override ISessionData SessionScopeData
        {
            get { return _sessionData; }
        }
    }
}
