using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.AuthorizationEntities
{
    public class ApplicationAuthenticationInfo : AggregateEntity, IApplicationAuthenticationInfo
    {
        private string _applicationLogOnName;
        private string _password;

        /// <summary>
        /// Gets or sets the name of the log on.
        /// </summary>
        /// <value>The name of the log on.</value>
        public virtual string ApplicationLogOnName
        {
            get
            {
                if (_applicationLogOnName == null)
                    _applicationLogOnName = string.Empty;
                return _applicationLogOnName;
            }
            set { _applicationLogOnName = value; }
        }

        /// <summary>
        /// Gets the password.
        /// </summary>
        /// <value>The password.</value>
        public virtual string Password
        {
            get
            {
                if (_password == null)
                    _password = string.Empty;
                return _password;
            }
            set { _password = value; }
        }

    }
}