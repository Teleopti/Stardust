using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.AuthorizationEntities
{
    /// <summary>
    /// Windows based authentication
    /// </summary>
    public class WindowsAuthenticationInfo : AggregateEntity, IWindowsAuthenticationInfo
    {
        private string _windowsLogOnName;
        private string _domainName;


        /// <summary>
        /// Gets or sets the name of the log on.
        /// </summary>
        /// <value>The name of the log on.</value>
        public virtual string WindowsLogOnName
        {
            get
            {
                if (_windowsLogOnName == null)
                    _windowsLogOnName = string.Empty;
                return _windowsLogOnName;
            }
            set { _windowsLogOnName = value; }
        }

        /// <summary>
        /// Gets or sets the name of the domain which holds this windows account.
        /// </summary>
        /// <value>The name of the domain.</value>
        public virtual string DomainName
        {
            get
            {
                if (_domainName == null)
                    _domainName = string.Empty;
                return _domainName;
            }
            set { _domainName = value; }
        }
    }
}