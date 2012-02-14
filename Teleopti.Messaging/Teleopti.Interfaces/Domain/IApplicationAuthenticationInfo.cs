
namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Username/password based authentication
    /// </summary>
    public interface IApplicationAuthenticationInfo : IAggregateEntity
    {
        /// <summary>
        /// Gets or sets the name of the log on.
        /// </summary>
        /// <value>The name of the log on.</value>
        string ApplicationLogOnName { get; set; }

        /// <summary>
        /// Gets the password.
        /// </summary>
        /// <value>The password.</value>
        string Password { get; set; }

    }
}
