using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Stores authentication data read in from configuration file. 
    /// </summary>
    public interface IAuthenticationSettings
    {
        /// <summary>
        /// Gets or sets the logon mode option.
        /// </summary>
        /// <value>The log on mode option.</value>
        LogOnModeOption LogOnMode { get; set; }
    }
}