using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
    /// <summary>
    /// Interface for specific methods on windows authentication.
    /// </summary>
    public interface IWindowsAuthenticator : IAuthenticator
    {
        /// <summary>
        /// Gets the name of the domain.
        /// </summary>
        /// <value>The name of the domain.</value>
        string DomainName { get; }

        /// <summary>
        /// Sets the log on values.
        /// </summary>
        /// <param name="domainName">Name of the domain.</param>
        /// <param name="logOnName">Name of the log on.</param>
        void SetLogOnValues(string domainName, string logOnName);
    }
}