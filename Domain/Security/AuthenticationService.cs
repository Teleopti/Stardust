using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;
using AuthenticationTypeOption=Teleopti.Interfaces.Domain.AuthenticationTypeOption;

namespace Teleopti.Ccc.Domain.Security
{
    /// <summary>
    /// Authentication service class.
    /// </summary>
    public class AuthenticationService : RepositoryService, IAuthenticationService
    {

        private IAuthenticator _lastUsedAuthenticator;
        private IApplicationAuthenticator _applicationAuthenticator;
        private IWindowsAuthenticator _windowsAuthenticator;
        private AuthenticationTypeOption _authenticationType = AuthenticationTypeOption.Unknown;

        private readonly ILogOnOff _logging;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationService"/> class.
        /// </summary>
        /// <param name="repFactory">The rep factory.</param>
        /// <param name="logging">The logging.</param>
        public AuthenticationService(IRepositoryFactory repFactory, ILogOnOff logging)
            : base(repFactory)
        {
            InParameter.NotNull("repFactory", repFactory);
            InParameter.NotNull("logging", logging);
            _logging = logging;
        }

        #region Interface

        public IAuthenticator Authenticator
        {
            protected set { _lastUsedAuthenticator = value; }
            get
            {
                switch (_authenticationType)
                {
                    case AuthenticationTypeOption.Application:
                        return _applicationAuthenticator;
                    case AuthenticationTypeOption.Windows:
                        return _windowsAuthenticator;                 
                    default:
                        return _lastUsedAuthenticator;
                }
            }
        }

        public AuthenticationTypeOption AuthenticationType
        {
            get { return _authenticationType; }
            set { _authenticationType = value; }
        }

        public IList<IDataSource> CreateLogonableDataSourcesListForWindowsUser(string domainName, string userName)
        {
            IList<IDataSource> logonableDatasources = new List<IDataSource>();

            // make sure that we have a IWindowsAuthenticator type
            IWindowsAuthenticator windowsAuthenticator = WindowsAuthenticator;

            windowsAuthenticator.SetLogOnValues(domainName, userName);
            windowsAuthenticator.DefineDataSources();

            foreach (IDataSource dataSources in windowsAuthenticator.LogonableDataSources)
                logonableDatasources.Add(dataSources);
            
            return logonableDatasources;
        }

        public IList<IDataSource> CreateAvailableDataSourcesListForApplicationUser()
        {
            IList<IDataSource> availableDatasources = new List<IDataSource>();

            // make sure that we have a IApplicationAuthenticator type
            IAuthenticator authenticator = WindowsAuthenticator;
            foreach (IDataSource dataSources in authenticator.AvailableDataSources)
                availableDatasources.Add(dataSources);

            return availableDatasources;
        }

        public StringCollection NotAvailableDataSourcesList
        {
            get
            {
                StringCollection result = new StringCollection();
                foreach (IDataSource source in Authenticator.RegisteredDataSources)
                {
                    if (!Authenticator.AvailableDataSources.Contains(source))
                        result.Add(source.Application.Name);
                }
                return result;
            }
        }

        public bool CheckDataSourceIsLogonableForApplicationUser(string selectedDataSource, string logOnName, string password)
        {
            // make sure that we have a IApplicationAuthenticator type
            IApplicationAuthenticator authenticator = ApplicationAuthenticator;

            foreach (IDataSource dataSource in authenticator.RegisteredDataSources)
            {
                if (dataSource.Application.Name == selectedDataSource)
                {
                    authenticator.SetLogOnValues(logOnName, password);
                    return authenticator.DefineDataSourceIsLogonable(dataSource);
                }
            }
            return false;
        }

        public IList<IBusinessUnit> CreateAvailableBusinessUnitCollection(string selectedDataSource)
        {
            // real environment
            foreach (IDataSource dataSource in Authenticator.LogonableDataSources)
            {
                if (dataSource.Application.Name == selectedDataSource)
                {
                    return Authenticator.AvailableBusinessUnits(dataSource);
                }
            }
            return null;
        }

        public void LogOn(string selectedDataSource, IBusinessUnit businessUnit)
        {
            foreach (IDataSource dataSources in Authenticator.LogonableDataSources)
            {
                if (dataSources.Application.Name == selectedDataSource)
                {
                    Authenticator.LogOn(dataSources, businessUnit);
                }
            }
        }

        public void LogOff()
        {
            if (Authenticator != null)
                Authenticator.LogOff();
        }

        #endregion

        #region Local

        /// <summary>
        /// Checks the and creates an windows authenticator if local WindowsAuthentication does not exist and 
        /// set the LastUsedAuthentication as well
        /// </summary>
        /// <returns></returns>
        protected IWindowsAuthenticator WindowsAuthenticator
        {
            get
            {
                if (_windowsAuthenticator == null)
                {
                    WindowsAuthenticator = new WindowsAuthenticator(RepositoryFactory, _logging);
                }
                return _windowsAuthenticator;
            }
            set
            {
                _windowsAuthenticator = value;
                Authenticator = value;
            }
        }

        /// <summary>
        /// Checks the and creates an application authenticator if local ApplicationAuthentication does not exist and 
        /// set the LastUsedAuthentication as well
        /// </summary>
        /// <returns></returns>
        protected IApplicationAuthenticator ApplicationAuthenticator
        {
            get
            {
                if (_applicationAuthenticator == null)
                {
                    ApplicationAuthenticator = new ApplicationAuthenticator(RepositoryFactory, _logging);
                }
                return _applicationAuthenticator;
            }
            set
            {
                _applicationAuthenticator = value;
                Authenticator = value;
            }
        }

        #endregion
    }
}
