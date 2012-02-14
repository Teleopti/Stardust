using System;

namespace Teleopti.Ccc.Domain.Repositories
{
    /// <summary>
    /// Interface for SLPS license functions.
    /// </summary>
    public interface ISlpsRepository: IDisposable
    {
        /// <summary>
        /// Gets a value indicating if a feature is enabled.
        /// </summary>
        /// <param name="featureName">Name of the feature.</param>
        /// <returns></returns>
        bool FeatureEnabled(string featureName);

        /// <summary>
        /// Closes the session.
        /// </summary>
        void CloseSession();
    }
}