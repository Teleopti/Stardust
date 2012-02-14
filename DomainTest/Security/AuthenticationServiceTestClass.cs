using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security
{
    /// <summary>
    /// Testable Authorization service.
    /// </summary>
    /// <remarks>
    /// Created by: tamasb
    /// Created date: 11/26/2007
    /// </remarks>
    public class AuthenticationServiceTestClass : AuthenticationService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationServiceTestClass"/> class.
        /// </summary>
        /// <param name="repFactory">The rep factory.</param>
        /// <param name="logging">The logging.</param>
        public AuthenticationServiceTestClass(IRepositoryFactory repFactory, ILogOnOff logging) : base(repFactory, logging)
        {
            //
        }
        /// <summary>
        /// Gets the authentication object.
        /// </summary>
        /// <value>The current authenticator.</value>
        public new IAuthenticator Authenticator
        {
            set { base.Authenticator = value; }
            get { return base.Authenticator; }
        }

        /// <summary>
        /// Checks the and creates an windows authenticator if local WindowsAuthentication does not exist and 
        /// set the LastUsedAuthentication as well
        /// </summary>
        /// <returns></returns>
        public new IWindowsAuthenticator WindowsAuthenticator
        {
            get
            {
                return base.WindowsAuthenticator;
            }
            set
            {
                base.WindowsAuthenticator = value;
            }
        }

        /// <summary>
        /// Checks the and creates an application authenticator if local ApplicationAuthentication does not exist and 
        /// set the LastUsedAuthentication as well
        /// </summary>
        /// <returns></returns>
        public new IApplicationAuthenticator ApplicationAuthenticator
        {
            get
            {
                return base.ApplicationAuthenticator;
            }
            set
            {
                base.ApplicationAuthenticator = value;
            }
        }

        ///// <summary>
        ///// Checks the and creates an application authenticator if local Authentication does not exist or not that type.
        ///// </summary>
        ///// <returns></returns>
        //public new IApplicationAuthenticator CheckAndCreateApplicationAuthenticator()
        //{
        //    return base.CheckAndCreateApplicationAuthenticator();
        //}

        ///// <summary>
        ///// Checks the and creates an windows authenticator if local Authentication does not exist or not that type.
        ///// </summary>
        ///// <returns></returns>
        //public new IWindowsAuthenticator CheckAndCreateWindowsAuthenticator()
        //{
        //    return base.CheckAndCreateWindowsAuthenticator();
        //}

    }
}
