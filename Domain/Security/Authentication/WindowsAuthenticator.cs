using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
    /// <summary>
    /// Windows authentication service
    /// </summary>
    public class WindowsAuthenticator : Authenticator, IWindowsAuthenticator
    {
        private string _domainName;
        private string _logOnName;

        /// <summary>
        /// Initializes a new instance of the <see cref="Authenticator"/> class.
        /// </summary>
        protected WindowsAuthenticator()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Authenticator"/> class.
        /// </summary>
        /// <param name="repFactory">The rep factory.</param>
        /// <param name="logging">The logging.</param>
        public WindowsAuthenticator(IRepositoryFactory repFactory, ILogOnOff logging)
            : base(repFactory, logging)
        {
        }

        /// <summary>
        /// Gets the current user's logged on authentication type.
        /// </summary>
        /// <value>The type of the authentication.</value>
        public override AuthenticationTypeOption AuthenticationType
        {
            get { return AuthenticationTypeOption.Windows; }
        }

        /// <summary>
        /// Gets the name of the domain.
        /// </summary>
        /// <value>The name of the domain.</value>
        public string DomainName
        {
            get { return _domainName; }
        }

        /// <summary>
        /// Gets the name of the log on.
        /// </summary>
        /// <value>The name of the log on.</value>
        public string LogOnName
        {
            get { return _logOnName; }
        }

        /// <summary>
        /// Sets the log on values.
        /// </summary>
        /// <param name="domainName">Name of the domain.</param>
        /// <param name="logOnName">Name of the log on.</param>
        public void SetLogOnValues(string domainName, string logOnName)
        {
            InParameter.NotNull("domainName", domainName);
            InParameter.NotNull("logOnName", logOnName);
            _domainName = domainName;
            _logOnName = logOnName;
        }

        /// <summary>
        /// Tries to find user in data base.
        /// This method should be implemented in proper way in concrete class
        /// </summary>
        /// <param name="personRepository">The user rep.</param>
        /// <param name="foundPerson">The found user.</param>
        /// <returns></returns>
        protected override bool TryFindPersonInDataSource(IPersonRepository personRepository, out IPerson foundPerson)
        {
            verifyLogOnValuesAreSet();
            return personRepository.TryFindWindowsAuthenticatedPerson(DomainName, LogOnName, out foundPerson);
        }

        private void verifyLogOnValuesAreSet()
        {
            if (DomainName == null || LogOnName == null)
                throw new PermissionException("Invalid operation when log on values are not set.");
        }
    }
}