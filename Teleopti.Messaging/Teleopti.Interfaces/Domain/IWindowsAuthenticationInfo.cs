namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Windows based authentication
    /// </summary>
    public interface IWindowsAuthenticationInfo : IAggregateEntity
    {
        /// <summary>
        /// Gets or sets the name of the log on.
        /// </summary>
        /// <value>The name of the log on.</value>
        string WindowsLogOnName { get; set; }

        /// <summary>
        /// Gets or sets the name of the domain which holds this windows account.
        /// </summary>
        /// <value>The name of the domain.</value>
        string DomainName { get; set; }
    }
}
