using System;
using log4net;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
    public class DataSourceContainer : IDataSourceContainer
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof (DataSourceContainer));
        private readonly IFindApplicationUser _checkLogOn;

        public DataSourceContainer(IDataSource dataSource, IRepositoryFactory repositoryFactory, IFindApplicationUser checkLogOn, AuthenticationTypeOption authenticationTypeOption)
        {
            _checkLogOn = checkLogOn;
            DataSource = dataSource;
            RepositoryFactory = repositoryFactory;
            AuthenticationTypeOption = authenticationTypeOption;
        }

        public IDataSource DataSource { get; private set; }

        public AuthenticationTypeOption AuthenticationTypeOption { get; private set; }

        public IRepositoryFactory RepositoryFactory { get; private set; }

        public IPerson User { get; private set; }

        public IAvailableBusinessUnitsProvider AvailableBusinessUnitProvider
        {
            get { return new AvailableBusinessUnitsProvider(this); }
        }

        public string DataSourceName 
        {
            get
            {
                return DataSource != null ? DataSource.DataSourceName : "";
            }
        } 

        public void SetUser(IPerson person)
        {
            User = person;
        }

	    
	    public string LogOnName { get; set; }

		
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public AuthenticationResult LogOn(string logOnName, string password)
        {
            AuthenticationResult result;
	        LogOnName = logOnName;
            using (var unitOfWork = DataSource.Application.CreateAndOpenUnitOfWork())
            {
                result = _checkLogOn.CheckLogOn(unitOfWork, logOnName, password);
                SetUser(result.Person);
				
                try
                {			
                    unitOfWork.PersistAll();
                }
                catch (Exception ex) //Not good at all! But we'll need to move the exceptions to domain first.
                {
                    _logger.Error(
                        string.Format(System.Globalization.CultureInfo.InvariantCulture,
                                      "Could not update information about log on session for user {0}.", logOnName), ex);
                }
                
            }
            return result;
        }

	    public AuthenticationResult LogOn(string identityLogOnName)
        {
            var result = new AuthenticationResult{Successful = false,HasMessage = false};
            using (var unitOfWork = DataSource.Application.CreateAndOpenUnitOfWork())
            {
                IPerson foundPerson;
                if (RepositoryFactory.CreatePersonRepository(unitOfWork).TryFindIdentityAuthenticatedPerson(identityLogOnName, out foundPerson))
                {
                    result.Successful = true;
                    result.Person = foundPerson;
                    SetUser(result.Person);
                }
            }
            return result;
        }

	    public override string ToString()
	    {
			return DataSource != null ? DataSource.DataSourceName : "";
	    }
    }
}