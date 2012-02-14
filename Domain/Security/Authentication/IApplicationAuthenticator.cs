using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
    /// <summary>
    /// Interface for application authentication functions.
    /// </summary>
    public interface IApplicationAuthenticator : IAuthenticator
    {

        /// <summary>
        /// Gets the password.
        /// </summary>
        /// <value>The pass word.</value>
        string Password { get; }

        /// <summary>
        /// Sets the log on values.
        /// </summary>
        /// <param name="logOnName">Name of the log on.</param>
        /// <param name="password">The password.</param>
        void SetLogOnValues(string logOnName, string password);
    }
}