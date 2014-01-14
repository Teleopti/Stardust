using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Security.Matrix
{
    /// <summary>
    /// Synchronizes the external matrix report functions with the matrix application functions in
    /// the Raptor database, using the externally defined functions as master. If there is a new  
    /// function in the external database then adds the function to the local database and sets its 
    /// default permissions. If a function has been deleted from the external database, it removes
    /// it from the local database as well. 
    /// </summary>
    public class MatrixReportsSynchronizer
    {
        #region Variables

        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IUnitOfWork _unitOfWork;

        private  IApplicationFunctionRepository _applicationFunctionRepository;
        private  IApplicationRoleRepository _applicationRoleRepository;
        private  IStatisticRepository _matrixRepository;       

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixReportsSynchronizer"/> class.
        /// </summary>
        /// <param name="repositoryFactory">The repository factory.</param>
        /// <param name="unitOfWork">The unit of work.</param>
        public MatrixReportsSynchronizer(IRepositoryFactory repositoryFactory, IUnitOfWork unitOfWork)
        {
            _repositoryFactory = repositoryFactory;
            _unitOfWork = unitOfWork;
        }

        #endregion

        #region Interface

        /// <summary>
        /// Digests the reports from matrix.
        /// </summary>
        public void SynchronizeMatrixReports()
        {
            CreateLocalRepositories();

            IList<IApplicationFunction> applicationFunctions = LoadApplicationFunctions(_applicationFunctionRepository);
            IList<IApplicationRole> applicationRoles = LoadApplicationRoles(_applicationRoleRepository);
            ICollection<MatrixReportInfo> matrixReports = LoadMatrixReports(_matrixRepository);

            IList<IApplicationFunction> filteredApplicationList =
                FilterExistingMatrixReportApplicationFunctions(applicationFunctions);

            IList<IApplicationFunction> deletedMatrixFunctions =
                new List<IApplicationFunction>(DeletedMatrixReports(filteredApplicationList, matrixReports));

            foreach (IApplicationFunction function in deletedMatrixFunctions)
            {
                RemoveMatrixApplicationFunctionFromRoles(applicationRoles, function);
                DeleteApplicationFunctionFromRepository(_applicationFunctionRepository, function);
                filteredApplicationList.Remove(function);
            }

            foreach (MatrixReportInfo info in AddedMatrixReports(matrixReports, filteredApplicationList))
            {
                IApplicationFunction newFunction = CreateMatrixReportApplicationFunction(applicationFunctions, info);
                AddApplicationFunctionToRepository(_applicationFunctionRepository, newFunction);
                filteredApplicationList.Add(newFunction);
            }

            foreach (IApplicationFunction applicationFunction in filteredApplicationList)
            {
                applicationFunction.Parent = ApplicationFunction.FindByPath(applicationFunctions, DefinedRaptorApplicationFunctionPaths.AccessToReports);
            }

            // Update report name/resource key
            UpdateMatrixApplicationFunctions(filteredApplicationList, matrixReports);

            PersistChangesToDatabase(_unitOfWork);
        }

        #endregion

        #region Local methods

        /// <summary>
        /// Creates the local repositories.
        /// </summary>
        protected virtual void CreateLocalRepositories()
        {
            _applicationFunctionRepository = _repositoryFactory.CreateApplicationFunctionRepository(_unitOfWork);
            _applicationRoleRepository = _repositoryFactory.CreateApplicationRoleRepository(_unitOfWork);
            _matrixRepository = _repositoryFactory.CreateStatisticRepository();
        }

        /// <summary>
        /// Loads all the matrix reports.
        /// </summary>
        /// <param name="statisticRepository">The matrix repository.</param>
        /// <returns></returns>
        protected virtual ICollection<MatrixReportInfo> LoadMatrixReports(IStatisticRepository statisticRepository)
        {
            return statisticRepository.LoadReports();
        }

        /// <summary>
        /// Loads all the application functions.
        /// </summary>
        /// <param name="applicationFunctionRepository">The app repository.</param>
        /// <returns></returns>
        protected virtual IList<IApplicationFunction> LoadApplicationFunctions(IApplicationFunctionRepository applicationFunctionRepository)
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
        /// Creates an application function from the matrix report parameter.
        /// </summary>
        /// <param name="applicationFunctions">The application functions.</param>
        /// <param name="info">The info.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.String.ToUpper")]
        protected virtual IApplicationFunction CreateMatrixReportApplicationFunction(IEnumerable<IApplicationFunction> applicationFunctions, MatrixReportInfo info)
        {
            IApplicationFunction newFunction = new ApplicationFunction();
            newFunction.Parent = ApplicationFunction.FindByPath(applicationFunctions, DefinedRaptorApplicationFunctionPaths.AccessToReports);
            newFunction.FunctionCode = RemoveLeadingXFromDescription(info.ReportName);
            newFunction.FunctionDescription = info.ReportName;
            newFunction.ForeignId = info.ReportId.ToString().ToUpper();
            newFunction.ForeignSource = DefinedForeignSourceNames.SourceMatrix;
            return newFunction;
        }

        protected virtual string RemoveLeadingXFromDescription(string descriptionText)
        {
            if (!String.IsNullOrEmpty(descriptionText))
            {
                if (descriptionText.StartsWith("xxx", StringComparison.OrdinalIgnoreCase))
                    descriptionText = descriptionText.Substring(3);
                if (descriptionText.StartsWith("xx", StringComparison.OrdinalIgnoreCase))
                    descriptionText = descriptionText.Substring(2);
            }
            return descriptionText;

        }

        /// <summary>
        /// Removes the matrix application function from all the roles where it has references.
        /// </summary>
        /// <param name="applicationRoles">The application roles.</param>
        /// <param name="applicationFunction">The application function.</param>
        protected virtual void RemoveMatrixApplicationFunctionFromRoles(IList<IApplicationRole> applicationRoles, IApplicationFunction applicationFunction)
        {
            applicationFunction.RemoveApplicationRoleFromContext(applicationRoles);
        }

        /// <summary>
        /// Yield enumeration on every matrix report where no matching application function is found, 
        /// which means that a new matrix report has been added in matrix database.
        /// </summary>
        /// <param name="matrixReports">The matrix reports.</param>
        /// <param name="functions">The functions.</param>
        /// <returns></returns>
        public virtual IEnumerable<MatrixReportInfo> AddedMatrixReports(IEnumerable<MatrixReportInfo> matrixReports, IEnumerable<IApplicationFunction> functions)
        {
            // matrixReports is null if Matrix database is not found
            if (matrixReports != null)
            {
                foreach (MatrixReportInfo info in matrixReports)
                {
                    IApplicationFunction foundFunction = ApplicationFunction.FindByForeignId(functions,
                                                                                             DefinedForeignSourceNames.
                                                                                                 SourceMatrix,
                                                                                             info.ReportId.ToString().ToUpper());
                    if (foundFunction == null)
                    {
                        yield return info;
                    }
                }
            }
        }

        /// <summary>
        /// Yield enumeration on every application function where no matching report Id is found, which 
        /// means that the matrix report has been deleted in matrix database.
        /// </summary>
        /// <param name="reportFunctions">The report functions.</param>
        /// <param name="matrixReports">The matrix reports.</param>
        /// <returns></returns>
        protected virtual IEnumerable<IApplicationFunction> DeletedMatrixReports(IEnumerable<IApplicationFunction> reportFunctions, IEnumerable<MatrixReportInfo> matrixReports)
        {
            // matrixReports is null if Matrix database is not found
            if (matrixReports != null)
            {
                foreach (IApplicationFunction function in reportFunctions)
                {
                    if (!string.IsNullOrEmpty(function.ForeignId) && isGuid(function.ForeignId))
                    {
                        MatrixReportInfo foundReport =
                            MatrixReportInfo.FindByReportId(matrixReports, new Guid(function.ForeignId.ToUpper()));
                        if (foundReport == null)
                        {
                            yield return function;
                        }
                        else
                        {
                            //Update resource key
                            function.FunctionDescription = foundReport.ReportName;
                        }
                    }
                }
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Guid"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static bool isGuid(string hopefullyGuid)
        {
            try
            {
                var guid = new Guid(hopefullyGuid);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// Filters to the existing matrix application functions.
        /// </summary>
        /// <param name="applicationFunctions">All application functions.</param>
        /// <returns></returns>
        protected virtual IList<IApplicationFunction> FilterExistingMatrixReportApplicationFunctions(IEnumerable<IApplicationFunction> applicationFunctions)
        {
            return new List<IApplicationFunction>(applicationFunctions)
                .FindAll(new ExternalApplicationFunctionSpecification(DefinedForeignSourceNames.SourceMatrix).IsSatisfiedBy);
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

        /// <summary>
        /// Updates the matrix application functions.
        /// </summary>
        /// <param name="functions">The functions.</param>
        /// <param name="matrixReports">The matrix reports.</param>
        /// <remarks>
        /// Created by: Jonas N
        /// Created date: 2008-12-18
        /// </remarks>
        protected virtual void UpdateMatrixApplicationFunctions(IEnumerable<IApplicationFunction> functions, IEnumerable<MatrixReportInfo> matrixReports)
        {
            foreach (IApplicationFunction function in functions)
            {
                if(!isGuid(function.ForeignId)) continue;

                MatrixReportInfo report =
                    MatrixReportInfo.FindByReportId(matrixReports, new Guid(function.ForeignId));
                if (report != null)
                {
                    function.FunctionCode = report.Version + RemoveLeadingXFromDescription(report.ReportName);
                    function.FunctionDescription = report.ReportName;
                }
            }
        }

        #endregion
    }
}