using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
    /// <summary>
    /// Stores authentication data read in from configuration file. 
    /// </summary>
    public class AuthenticationSettings : IAuthenticationSettings
    {
        private LogOnModeOption _logOnMode;

        /// <summary>
        /// Gets or sets the logon mode option.
        /// </summary>
        /// <value>The log on mode option.</value>
        public LogOnModeOption LogOnMode
        {
            get { return _logOnMode; }
            set { _logOnMode = value; }
        }
    }
}
