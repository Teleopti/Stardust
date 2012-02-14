using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.AuthorizationEntities
{
    /// <summary>
    /// Interface for permission infos.
    /// Used to describe different ways to authenticate.
    /// </summary>
    public interface IAuthenticationInfo : IAggregateEntity
    {
        /// <summary>
        /// Gets or sets the name of the log on.
        /// </summary>
        /// <value>The name of the log on.</value>
        string LogOnName { get; set; }
    }
}