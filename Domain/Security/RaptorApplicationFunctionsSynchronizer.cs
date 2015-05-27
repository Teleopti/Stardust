using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Security
{
	public interface IRaptorApplicationFunctionsSynchronizer
	{
		/// <summary>
		/// Digests the reports from matrix.
		/// </summary>
		CheckRaptorApplicationFunctionsResult CheckRaptorApplicationFunctions();

		/// <summary>
		/// Digests the reports from matrix.
		/// </summary>
		void DigestApplicationFunctions();

		/// <summary>
		/// Yield enumeration on every defined application function where no matching database
		/// pair found 
		/// </summary>
		/// <param name="databaseFunctions">The database functions.</param>
		/// <param name="definedFunctions">The defined functions.</param>
		/// <returns></returns>
		IEnumerable<IApplicationFunction> AddedRaptorApplicationFunctions(ICollection<IApplicationFunction> databaseFunctions, IEnumerable<IApplicationFunction> definedFunctions);

		/// <summary>
		/// Yield enumeration on every application function in the database where no matching pre-defined
		/// pair found 
		/// </summary>
		/// <param name="definedFunctions">The defined functions.</param>
		/// <param name="databaseFunctions">The database functions.</param>
		/// <returns></returns>
		IEnumerable<IApplicationFunction> DeletedRaptorApplicationFunctions(ICollection<IApplicationFunction> definedFunctions, IEnumerable<IApplicationFunction> databaseFunctions);

		/// <summary>
		/// Filters to the existing pre-defined application functions.
		/// </summary>
		/// <param name="applicationFunctions">All application functions.</param>
		/// <returns></returns>
		IList<IApplicationFunction> FilterExistingDefinedRaptorApplicationFunctions(IEnumerable<IApplicationFunction> applicationFunctions);
	}

	/// <summary>
    /// Synchronizes the code-defined application functions with the application functions in
    /// the database, using the code-defined functions as master. If there is a new code-defined 
    /// function then adds to the database and sets its permissions. If a function has been deleted
    /// from the code, it removes it from the database. 
    /// </summary>
    public class RaptorApplicationFunctionsSynchronizer : IRaptorApplicationFunctionsSynchronizer
	{
        private readonly IRepositoryFactory _repFactory;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

        private  IApplicationFunctionRepository _applicationFunctionRepository;
        private  IApplicationRoleRepository _applicationRoleRepository;
        private  IAvailableDataRepository _availableDataRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="RaptorApplicationFunctionsSynchronizer"/> class.
        /// </summary>
        /// <param name="repFactory">The rep factory.</param>
        /// <param name="unitOfWorkFactory">The unit of work factory.</param>
        public RaptorApplicationFunctionsSynchronizer(IRepositoryFactory repFactory, ICurrentUnitOfWorkFactory unitOfWorkFactory)
        {
            _repFactory = repFactory;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        /// <summary>
        /// Digests the reports from matrix.
        /// </summary>
		public CheckRaptorApplicationFunctionsResult CheckRaptorApplicationFunctions()
        {
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
            {
                
                _applicationFunctionRepository = _repFactory.CreateApplicationFunctionRepository(unitOfWork);

                IList<IApplicationFunction> definedRaptorApplicationFunctions = LoadDefinedRaptorApplicationFunctions();
                IList<IApplicationFunction> databaseApplicationFunctions =
                    LoadApplicationFunctionsFromDatabase(_applicationFunctionRepository);

                ICollection<IApplicationFunction> databaseRaptorApplicationFunctions =
                    FilterExistingDefinedRaptorApplicationFunctions(databaseApplicationFunctions);


                var addedFunctions =
                    AddedRaptorApplicationFunctions(databaseRaptorApplicationFunctions, definedRaptorApplicationFunctions);

                var deletedFunctions =
                    DeletedRaptorApplicationFunctions(definedRaptorApplicationFunctions, databaseRaptorApplicationFunctions);

				return new CheckRaptorApplicationFunctionsResult(addedFunctions, deletedFunctions);

            }
        }

        /// <summary>
        /// Digests the reports from matrix.
        /// </summary>
        public void DigestApplicationFunctions()
        {
			using (IUnitOfWork unitOfWork = _unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
            {
                CreateLocalRepositories(unitOfWork);

                IList<IApplicationFunction> definedRaptorApplicationFunctions = LoadDefinedRaptorApplicationFunctions();
                IList<IApplicationFunction> databaseApplicationFunctions =
                    LoadApplicationFunctionsFromDatabase(_applicationFunctionRepository);
                IList<IApplicationRole> applicationRoles = LoadApplicationRoles(_applicationRoleRepository);
                IList<IAvailableData> availableDataList = LoadAvailableDataList(_availableDataRepository);

                ICollection<IApplicationFunction> databaseRaptorApplicationFunctions =
                    FilterExistingDefinedRaptorApplicationFunctions(databaseApplicationFunctions);

                IList<IApplicationFunction> deletedFunctions =
                    new List<IApplicationFunction>(DeletedRaptorApplicationFunctions(definedRaptorApplicationFunctions,
                                                                                     databaseRaptorApplicationFunctions));

                // Delete
                foreach (ApplicationFunction applicationFunction in deletedFunctions)
                {
                    RemoveApplicationFunctionFromRoles(applicationRoles, applicationFunction);
                    DeleteApplicationFunctionFromRepository(_applicationFunctionRepository, applicationFunction);
                    databaseRaptorApplicationFunctions.Remove(applicationFunction);
                }
                
                // Add
                foreach (ApplicationFunction applicationFunction in AddedRaptorApplicationFunctions(databaseRaptorApplicationFunctions, definedRaptorApplicationFunctions))
                {
                    IApplicationFunction newApplicationFunction = new ApplicationFunction();
                    newApplicationFunction.FunctionCode = applicationFunction.FunctionCode;
                    newApplicationFunction.FunctionDescription = applicationFunction.FunctionDescription;
                    newApplicationFunction.ForeignId = applicationFunction.ForeignId;
                    newApplicationFunction.ForeignSource = applicationFunction.ForeignSource;
                    newApplicationFunction.SortOrder = applicationFunction.SortOrder;
                    IApplicationFunction parent = ApplicationFunction.FindByPath(databaseRaptorApplicationFunctions,
                                                                                 ApplicationFunction.GetParentPath(
                                                                                     applicationFunction.FunctionPath));
                    newApplicationFunction.Parent = parent;
                    databaseRaptorApplicationFunctions.Add(newApplicationFunction);
                    AddApplicationFunctionToRepository(_applicationFunctionRepository, newApplicationFunction);
                    AssignApplicationFunctionToRoles(availableDataList, newApplicationFunction);
                }

                // Update
                foreach (IApplicationFunction applicationFunction in databaseRaptorApplicationFunctions)
                {
                    IApplicationFunction raptorCounterpart =
                        ApplicationFunction.FindByForeignId(definedRaptorApplicationFunctions,
                                                            DefinedForeignSourceNames.SourceRaptor,
                                                            applicationFunction.ForeignId);
                    IApplicationFunction raptorParent = 
                        ApplicationFunction.FindByPath(definedRaptorApplicationFunctions,
                                                       ApplicationFunction.GetParentPath(raptorCounterpart.FunctionPath));
                    
                    if (raptorParent != null)
                    {
                        IApplicationFunction parent =
                            ApplicationFunction.FindByForeignId(databaseRaptorApplicationFunctions,
                                                                DefinedForeignSourceNames.SourceRaptor,
                                                                raptorParent.ForeignId);
                        applicationFunction.Parent = parent;
                    }
                    else
                        applicationFunction.Parent= null;
                }

                PersistChangesToDatabase(unitOfWork);
            }
        }

        /// <summary>
        /// Creates the local repositories.
        /// </summary>
        protected virtual void CreateLocalRepositories(IUnitOfWork unitOfWork)
        {
            _applicationFunctionRepository = _repFactory.CreateApplicationFunctionRepository(unitOfWork);
            _applicationRoleRepository = _repFactory.CreateApplicationRoleRepository(unitOfWork);
            _availableDataRepository = _repFactory.CreateAvailableDataRepository(unitOfWork);
        }

        /// <summary>
        /// Loads all the pre-defined application functions.
        /// </summary>
        /// <returns></returns>
        protected virtual IList<IApplicationFunction> LoadDefinedRaptorApplicationFunctions()
        {
            return new DefinedRaptorApplicationFunctionFactory().ApplicationFunctionList.ToList();
        }

        /// <summary>
        /// Loads all the application functions that are persisted in the database.
        /// </summary>
        /// <param name="applicationFunctionRepository">The app repository.</param>
        /// <returns></returns>
        protected virtual IList<IApplicationFunction> LoadApplicationFunctionsFromDatabase(IApplicationFunctionRepository applicationFunctionRepository)
        {
            return applicationFunctionRepository.GetAllApplicationFunctionSortedByCode();
        }

        /// <summary>
        /// Loads all the application roles.
        /// </summary>
        /// <param name="applicationRoleRepository">The application role repository.</param>
        /// <returns></returns>
        protected virtual IList<IApplicationRole> LoadApplicationRoles(IApplicationRoleRepository applicationRoleRepository)
        {
            return applicationRoleRepository.LoadAllApplicationRolesSortedByName();
        }

        /// <summary>
        /// Loads all the available data.
        /// </summary>
        /// <param name="availableDataRepository">The available data repository.</param>
        /// <returns></returns>
        protected virtual IList<IAvailableData> LoadAvailableDataList(IAvailableDataRepository availableDataRepository)
        {
            return availableDataRepository.LoadAllAvailableData();
        }

        /// <summary>
        /// Assign the application function to all the relevant application roles where the
        /// available data rights are equal or higher than Site.
        /// </summary>
        /// <param name="availableDataList">The available datas.</param>
        /// <param name="applicationFunction">The application function.</param>
        protected virtual void AssignApplicationFunctionToRoles(IList<IAvailableData> availableDataList, IApplicationFunction applicationFunction)
        {
            foreach (IAvailableData availableData in availableDataList)
            {
                if(availableData.ApplicationRole != null)
                {
                    if(availableData.AvailableDataRange >= AvailableDataRangeOption.MyBusinessUnit)
                    availableData.ApplicationRole.AddApplicationFunction(applicationFunction);
                }
            }
        }

        /// <summary>
        /// Removes the application function from all the roles where it has references.
        /// </summary>
        /// <param name="applicationRoles">The application roles.</param>
        /// <param name="applicationFunction">The application function.</param>
        protected virtual void RemoveApplicationFunctionFromRoles(IList<IApplicationRole> applicationRoles, IApplicationFunction applicationFunction)
        {
            applicationFunction.RemoveApplicationRoleFromContext(applicationRoles);
        }

        /// <summary>
        /// Yield enumeration on every defined application function where no matching database
        /// pair found 
        /// </summary>
        /// <param name="databaseFunctions">The database functions.</param>
        /// <param name="definedFunctions">The defined functions.</param>
        /// <returns></returns>
        public virtual IEnumerable<IApplicationFunction> AddedRaptorApplicationFunctions(ICollection<IApplicationFunction> databaseFunctions, IEnumerable<IApplicationFunction> definedFunctions)
        {
            foreach (IApplicationFunction applicationFunction in definedFunctions)
            {
                if (ApplicationFunction.FindByForeignId(databaseFunctions, applicationFunction.ForeignSource, applicationFunction.ForeignId) == null)
                {
                    yield return applicationFunction;
                }
            }
        }

        /// <summary>
        /// Yield enumeration on every application function in the database where no matching pre-defined
        /// pair found 
        /// </summary>
        /// <param name="definedFunctions">The defined functions.</param>
        /// <param name="databaseFunctions">The database functions.</param>
        /// <returns></returns>
        public virtual IEnumerable<IApplicationFunction> DeletedRaptorApplicationFunctions(ICollection<IApplicationFunction> definedFunctions, IEnumerable<IApplicationFunction> databaseFunctions)
        {
            foreach (IApplicationFunction applicationFunction in databaseFunctions)
            {
                if (ApplicationFunction.FindByForeignId(definedFunctions, applicationFunction.ForeignSource, applicationFunction.ForeignId) == null)
                {
                    yield return applicationFunction;
                }
            }
        }

        /// <summary>
        /// Filters to the existing pre-defined application functions.
        /// </summary>
        /// <param name="applicationFunctions">All application functions.</param>
        /// <returns></returns>
        public virtual IList<IApplicationFunction> FilterExistingDefinedRaptorApplicationFunctions(IEnumerable<IApplicationFunction> applicationFunctions)
        {
            return new List<IApplicationFunction>(applicationFunctions)
                .FindAll(new ExternalApplicationFunctionSpecification(DefinedForeignSourceNames.SourceRaptor).IsSatisfiedBy);

        }

        /// <summary>
        /// Adds the application function to the application repository.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="applicationFunction">The application function.</param>
        protected virtual void AddApplicationFunctionToRepository(IApplicationFunctionRepository repository, IApplicationFunction applicationFunction)
        {
            repository.Add(applicationFunction);
        }

        /// <summary>
        /// Adds the application function to the application repository.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="applicationFunction">The application function.</param>
        protected virtual void DeleteApplicationFunctionFromRepository(IApplicationFunctionRepository repository, IApplicationFunction applicationFunction)
        {
            repository.Remove(applicationFunction);
        }

        /// <summary>
        /// Saves the application functions and treir assignments to database.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        protected virtual void PersistChangesToDatabase(IUnitOfWork unitOfWork)
        {
            unitOfWork.PersistAll();
        }
    }


	public class CheckRaptorApplicationFunctionsResult
	{
		private readonly IEnumerable<IApplicationFunction> _addedFunctions;
		private readonly IEnumerable<IApplicationFunction> _deletedFunctions;

		public CheckRaptorApplicationFunctionsResult(
			IEnumerable<IApplicationFunction> addedFunctions,
			IEnumerable<IApplicationFunction> deletedFunctions)
		{
			_addedFunctions = addedFunctions;
			_deletedFunctions = deletedFunctions;
		}

		public IEnumerable<IApplicationFunction> AddedFunctions
		{
			get { return _addedFunctions; }
		}

		public IEnumerable<IApplicationFunction> DeletedFunctions
		{
			get { return _deletedFunctions; }
		}

		public bool Result
		{
			get
			{
				if (_addedFunctions.Any() || _deletedFunctions.Any())
					return false;
				return true;
			}
		}
	}
}
