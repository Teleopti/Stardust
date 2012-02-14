namespace Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers
{
    /// <summary>
    /// Interface for holding different kind of states.
    /// </summary>
    /// <remarks>
    /// Created by: Dinesh Ranasinghe
    /// Created date: 2008-03-03
    /// </remarks>
    public interface IState:IStateReader
    {
        /// <summary>
        /// Sets the session data.
        /// </summary>
        /// <param name="sessionData">The session data.</param>
        void SetSessionData(ISessionData sessionData);

        /// <summary>
        /// Clears data kept for logged in session/user
        /// </summary>
        void ClearSession();
    }
}