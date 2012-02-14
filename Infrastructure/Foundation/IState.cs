using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
    /// <summary>
    /// Interface for holding different kinds of state
    /// </summary>
    public interface IState : IStateReader
    {
        /// <summary>
        /// Sets the session data.
        /// </summary>
        /// <param name="sessionData">The session data.</param>
        void SetSessionData(ISessionData sessionData);

        /// <summary>
        /// Sets the application data.
        /// </summary>
        /// <param name="applicationData">The application data.</param>
        void SetApplicationData(IApplicationData applicationData);
    }
}