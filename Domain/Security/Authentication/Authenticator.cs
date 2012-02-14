using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
    /// <summary>
    /// Abstract class for authentication
    /// </summary>
    public abstract class Authenticator : RepositoryService, IAuthenticator
    {
        private IDictionary<IDataSource, IPerson> _logonableDataSources;
        private IList<IDataSource> _availableDataSources;
        private ILogOnOff _logging;

        /// <summary>
        /// Initializes a new instance of the <see cref="Authenticator"/> class.
        /// </summary>
        protected Authenticator()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Authenticator"/> class.
        /// </summary>
        /// <param name="repFactory">The rep factory.</param>
        /// <param name="logging">The logging.</param>
        protected Authenticator(IRepositoryFactory repFactory, ILogOnOff logging)
            : base(repFactory)
        {
            _logging = logging;
        }

        /// <summary>
        /// Tries to find person in the datasource.
        /// This method should be implemented in proper way in concrete class
        /// </summary>
        /// <param name="personRepository">The person repository.</param>
        /// <param name="foundPerson">The found person.</param>
        /// <returns></returns>
        protected abstract bool TryFindPersonInDataSource(IPersonRepository personRepository, out IPerson foundPerson);

        public virtual AuthenticationTypeOption AuthenticationType
        {
            get { return AuthenticationTypeOption.Unknown; }
        }

        public void LogOff()
        {
            _logging.LogOff();
        }

        public virtual ReadOnlyCollection<IDataSource> LogonableDataSources
        {
            get
            {
                if (_logonableDataSources == null)
                    throw new PermissionException("LogonableDataSources must be defined before they can be read.");
                return
                    new ReadOnlyCollection<IDataSource>(new List<IDataSource>(_logonableDataSources.Keys));
            }
        }

        public virtual IPerson LoggedOnPerson(IDataSource dataSource)
        {
            if (_logonableDataSources != null && _logonableDataSources.ContainsKey(dataSource))
            {
                return _logonableDataSources[dataSource];
            }
            throw new PermissionException("Invalid person.");
        }

        public virtual IEnumerable<IDataSource> RegisteredDataSources
        {
            get
            {
                return StateHolderReader.Instance.StateReader.ApplicationScopeData.RegisteredDataSourceCollection;
            }
        }

        public virtual ReadOnlyCollection<IDataSource> AvailableDataSources
        {
            get { return new ReadOnlyCollection<IDataSource>(_availableDataSources); }
        }

        public virtual void DefineDataSources()
        {
            _availableDataSources = new List<IDataSource>();
            _logonableDataSources = new Dictionary<IDataSource, IPerson>();
            foreach (IDataSource dataSource in RegisteredDataSources)
            {
                IPerson tempUser;
                UnitOfWorkFactoryValidationResult result = ValidateUnitOfWorkFactory(dataSource.Application, out tempUser);
                if (result > UnitOfWorkFactoryValidationResult.DatabaseNotAvailable)
                    _availableDataSources.Add(dataSource);
                if (result == UnitOfWorkFactoryValidationResult.Valid)
                    _logonableDataSources[dataSource] = tempUser;
            }
        }

        public virtual bool DefineDataSourceIsLogonable(IDataSource dataSource)
        {
            _logonableDataSources = new Dictionary<IDataSource, IPerson>();
            IPerson tempUser;
            UnitOfWorkFactoryValidationResult result = ValidateUnitOfWorkFactory(dataSource.Application, out tempUser);
            if (result == UnitOfWorkFactoryValidationResult.Valid)
            {
                _logonableDataSources[dataSource] = tempUser;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Logs on the system.
        /// </summary>
        /// <param name="dataSource">The uow factory.</param>
        /// <param name="businessUnit">The business unit.</param>
        public virtual void LogOn(IDataSource dataSource, IBusinessUnit businessUnit)
        {
            if (!_logonableDataSources.ContainsKey(dataSource) ||
                !AvailableBusinessUnits(dataSource).Contains(businessUnit) ||
                !businessUnit.Id.HasValue)
                throw new PermissionException("User has no permission to specific data source and business unit.");
            IPerson loggedOnUser = LoggedOnPerson(dataSource);
            using (IUnitOfWork uow = dataSource.Application.CreateAndOpenUnitOfWork())
            {
                IBusinessUnitRepository businessUnitRepository = RepositoryFactory.CreateBusinessUnitRepository(uow);
                uow.Reassociate(businessUnit);
                businessUnitRepository.LoadHierarchyInformation(businessUnit);
            }
            _logging.LogOn(dataSource, loggedOnUser, businessUnit);
        }

        public IList<IBusinessUnit> AvailableBusinessUnits(IDataSource dataSource)
        {
            IPerson person = LoggedOnPerson(dataSource);
            if (person.PermissionInformation.HasAccessToAllBusinessUnits())
            {
                using(IUnitOfWork uow = dataSource.Application.CreateAndOpenUnitOfWork())
                {
                    IBusinessUnitRepository businessUnitRepository = RepositoryFactory.CreateBusinessUnitRepository(uow);
                    return businessUnitRepository.LoadAllBusinessUnitSortedByName();
                }
            }
            
            return person.PermissionInformation.BusinessUnitAccessCollection();
        }

        /// <summary>
        /// Sets the logonable datasource collection.
        /// Put here for testing purposes
        /// </summary>
        /// <param name="logonableDataSources">The logonable datasource collection.</param>
        protected virtual void SetLogonableDataSourcesExplicitly(IDictionary<IDataSource, IPerson> logonableDataSources)
        {
            _logonableDataSources = logonableDataSources;
        }

        /// <summary>
        /// Validates the unit of work factory against the database connectivity and the specified user.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="foundUser">The found user.</param>
        /// <returns></returns>
        /// <remarks>
        /// The following SQL server errors are considered as a DatabaseNotAvailable type of errors and not considered as
        /// fatal error.
        /// <list type="bullet">
        ///     <item>
        ///         Error no: 4060. "Invalid database"
        ///     </item>
        ///     <item>
        ///         Error no: 53. "Network address not found"
        ///     </item>
        /// </list>
        /// As a result the connection that throws the errors above are just left out of the database connection list.
        /// </remarks>
        protected UnitOfWorkFactoryValidationResult ValidateUnitOfWorkFactory(IUnitOfWorkFactory factory, out IPerson foundUser)
        {
            IPersonRepository userRep;
            foundUser = null;
            UnitOfWorkFactoryValidationResult ret;
            IUnitOfWork uow = null;
            try
            {
                
                uow = factory.CreateAndOpenUnitOfWork();
                userRep = RepositoryFactory.CreatePersonRepository(uow);
                if (TryFindPersonInDataSource(userRep, out foundUser))
                    ret = UnitOfWorkFactoryValidationResult.Valid;
                else
                    ret = UnitOfWorkFactoryValidationResult.NoPersonFound;
            }
            catch (Exception ex)
            {
                SqlException sqlEx = ex.InnerException as SqlException;
                if (sqlEx != null && (sqlEx.Number == 4060 || sqlEx.Number == 53 || sqlEx.Number == -1 || sqlEx.Number == -2))
                    ret = UnitOfWorkFactoryValidationResult.DatabaseNotAvailable;
                else
                    throw;
            }
            finally
            {
                if (uow != null)
                    uow.Dispose();
            }
            return ret;
        }

    }
}