using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
    /// <summary>
    /// Interface for holding different kinds of state
    /// </summary>
    public interface IState : IStateReader
    {
        /// <summary>
        /// Sets the application data.
        /// </summary>
        /// <param name="applicationData">The application data.</param>
        void SetApplicationData(IApplicationData applicationData);
    }
}