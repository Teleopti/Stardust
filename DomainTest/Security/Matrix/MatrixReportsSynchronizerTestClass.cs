using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Security.Matrix
{
    public class MatrixReportsSynchronizerTestClass : MatrixReportsSynchronizer
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixReportsSynchronizerTestClass"/> class.
        /// </summary>
        /// <param name="repositoryFactory">The repository factory.</param>
        /// <param name="unitOfWork">The unit of work.</param>
        public MatrixReportsSynchronizerTestClass(IRepositoryFactory repositoryFactory, IUnitOfWork unitOfWork)
            : base(repositoryFactory, unitOfWork)
        {
            //
        }

        /// <summary>
        /// Makes the base class method testable.
        /// </summary>
        public new void CreateLocalRepositories()
        {
            base.CreateLocalRepositories();
        }

        /// <summary>
        /// Makes the base class method testable.
        /// </summary>
        /// <param name="repository">The application function repository.</param>
        /// <returns></returns>
        public new IList<IApplicationFunction> LoadApplicationFunctions(IApplicationFunctionRepository repository)
        {
            return base.LoadApplicationFunctions(repository);
        }

        /// <summary>
        /// Makes the base class method testable.
        /// </summary>
        /// <param name="applicationRoleRepository">The application role repository.</param>
        /// <returns></returns>
        public new IList<IApplicationRole> LoadApplicationRoles(IApplicationRoleRepository applicationRoleRepository)
        {
            return base.LoadApplicationRoles(applicationRoleRepository);
        }

        /// <summary>
        /// Loads all the available data.
        /// </summary>
        /// <param name="availableDataRepository">The available data repository.</param>
        /// <returns></returns>
        public new IList<IAvailableData> LoadAvailableDataList(IAvailableDataRepository availableDataRepository)
        {
            return base.LoadAvailableDataList(availableDataRepository);
        }


        /// <summary>
        /// Makes the base class method testable.
        /// </summary>
        /// <param name="applicationFunctions">The application functions.</param>
        /// <param name="info">The matrix report info.</param>
        /// <returns></returns>
        public new IApplicationFunction CreateMatrixReportApplicationFunction(IEnumerable<IApplicationFunction> applicationFunctions, MatrixReportInfo info)
        {
            return base.CreateMatrixReportApplicationFunction(applicationFunctions, info);
        }

        /// <summary>
        /// Makes the base class method testable.
        /// </summary>
        /// <param name="applicationRoles">The application roles.</param>
        /// <param name="applicationFunction">The application function.</param>
        public new void RemoveMatrixApplicationFunctionFromRoles(IList<IApplicationRole> applicationRoles, IApplicationFunction applicationFunction)
        {
            base.RemoveMatrixApplicationFunctionFromRoles(applicationRoles, applicationFunction);
        }

        /// <summary>
        /// Makes the base class method testable.
        /// </summary>
        /// <param name="functions">The functions.</param>
        /// <param name="matrixReports">The matrix reports.</param>
        /// <returns></returns>
        public new IEnumerable<IApplicationFunction> DeletedMatrixReports(IEnumerable<IApplicationFunction> functions, IEnumerable<MatrixReportInfo> matrixReports)
        {
            return base.DeletedMatrixReports(functions, matrixReports);
        }

        /// <summary>
        /// Makes the base class method testable.
        /// </summary>
        /// <param name="applicationFunctions">All application functions.</param>
        /// <returns></returns>
        public new IList<IApplicationFunction> FilterExistingMatrixReportApplicationFunctions(IEnumerable<IApplicationFunction> applicationFunctions)
        {
            return base.FilterExistingMatrixReportApplicationFunctions(applicationFunctions);
        }

        /// <summary>
        /// Makes the base class method testable.
        /// </summary>
        /// <param name="statisticRepository">The matrix repository.</param>
        /// <returns></returns>
        public new virtual ICollection<MatrixReportInfo> LoadMatrixReports(IStatisticRepository statisticRepository)
        {
            return base.LoadMatrixReports(statisticRepository);
        }

        /// <summary>
        /// Makes the base class method testable.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="applicationFunction">The application function.</param>
        public new void AddApplicationFunctionToRepository(IApplicationFunctionRepository repository, IApplicationFunction applicationFunction)
        {
            base.AddApplicationFunctionToRepository(repository, applicationFunction);
        }

        /// <summary>
        /// Makes the base class method testable.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="applicationFunction">The application function.</param>
        public new void DeleteApplicationFunctionFromRepository(IApplicationFunctionRepository repository, IApplicationFunction applicationFunction)
        {
            base.DeleteApplicationFunctionFromRepository(repository, applicationFunction);
        }

        /// <summary>
        /// Makes the base class method testable.
        /// </summary>
        /// <param name="functions">The functions.</param>
        /// <param name="matrixReports">The matrix reports.</param>
        public new void UpdateMatrixApplicationFunctions(IEnumerable<IApplicationFunction> functions, IEnumerable<MatrixReportInfo> matrixReports)
        {
            base.UpdateMatrixApplicationFunctions(functions, matrixReports);
        }
    }
}