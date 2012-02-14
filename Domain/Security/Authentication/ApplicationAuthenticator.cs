using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.Domain.Security.Authentication
{
    /// <summary>
    /// Service with methods helping to logon
    /// to the system with Basic Authentication
    /// </summary>
    public class ApplicationAuthenticator : Authenticator, IApplicationAuthenticator
    {
        private string _logOnName;
        private string _password;

        /// <summary>
        /// Initializes a new instance of the <see cref="Authenticator"/> class.
        /// </summary>
        protected ApplicationAuthenticator()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Authenticator"/> class.
        /// </summary>
        /// <param name="repFactory">The rep factory.</param>
        /// <param name="logging">The logging.</param>
        public ApplicationAuthenticator(IRepositoryFactory repFactory, ILogOnOff logging)
            : base(repFactory, logging)
        {
        }

        /// <summary>
        /// Gets the current user's logged on authentication type.
        /// </summary>
        /// <value>The type of the authentication.</value>
        public override AuthenticationTypeOption AuthenticationType
        {
            get { return AuthenticationTypeOption.Application; }
        }

        /// <summary>
        /// Gets the ApplicationLogOnName.
        /// </summary>
        /// <value>The name of the logon.</value>
        public string LogOnName
        {
            get { return _logOnName; }
        }

        /// <summary>
        /// Gets the password.
        /// </summary>
        /// <value>The pass word.</value>
        public string Password
        {
            get { return _password; }
        }

        /// <summary>
        /// Sets the log on values.
        /// </summary>
        /// <param name="logOnName">Name of the log on.</param>
        /// <param name="password">The password.</param>
        public void SetLogOnValues(string logOnName, string password)
        {
            InParameter.NotNull("logOnName", logOnName);
            InParameter.NotNull("password", password);
            _logOnName = logOnName;
            _password = password;
        }

        /// <summary>
        /// Gets the possible data source candidates.
        /// </summary>
        /// <value>The possible data sources.</value>
        public override IEnumerable<IDataSource> RegisteredDataSources
        {
            get
            {
                IList<IDataSource> result = new List<IDataSource>();
                foreach (IDataSource dataSource in StateHolderReader.Instance.StateReader.ApplicationScopeData.RegisteredDataSourceCollection)
                {
                    if(dataSource.AuthenticationSettings.LogOnMode == LogOnModeOption.Mix)
                    {
                        result.Add(dataSource);
                    }
                }
                return new ReadOnlyCollection<IDataSource>(result);
            }
        }

        /// <summary>
        /// Tries to find person in data base based on basic authentication.
        /// </summary>
        /// <param name="personRepository">The person repository.</param>
        /// <param name="foundPerson">The found person.</param>
        /// <returns><c>true</c> if person exists</returns>
        protected override bool TryFindPersonInDataSource(IPersonRepository personRepository, out IPerson foundPerson)
        {
            verifyLogOnValuesAreSet();
            return personRepository.TryFindBasicAuthenticatedPerson(LogOnName, Password, out foundPerson);
        }

        private void verifyLogOnValuesAreSet()
        {
            if (Password == null || LogOnName == null)
                throw new PermissionException("Invalid operation when log on values are not set.");
        }
    }
}