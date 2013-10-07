using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.DomainTest.Security.Matrix;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Security
{
    public class RaptorApplicationFunctionsSynchronizerTestClass : RaptorApplicationFunctionsSynchronizer
    {
        private IList<IApplicationFunction> _definedApplicationFunctions;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixReportsSynchronizerTestClass"/> class.
        /// </summary>
        /// <param name="repositoryFactory">The repository factory.</param>
        /// <param name="unitOfWorkFactory">The unit of work factory.</param>
        public RaptorApplicationFunctionsSynchronizerTestClass(IRepositoryFactory repositoryFactory, ICurrentUnitOfWorkFactory unitOfWorkFactory)
            : base(repositoryFactory, unitOfWorkFactory)
        {
            //
        }

        /// <summary>
        /// Runs the base method with injecting the definedApplicationFunctions parameter.
        /// </summary>
        /// <param name="definedApplicationFunctions">The defined application functions.</param>
        /// <remarks>
        /// Injects the definedApplicationFunctions parameter to the real class. See the 
        /// overriden LoadDefinedRaptorApplicationFunctions method.
        /// </remarks>
        public void DigestApplicationFunctions(IList<IApplicationFunction> definedApplicationFunctions)
        {
            _definedApplicationFunctions = definedApplicationFunctions;
            DigestApplicationFunctions();
        }

        /// <summary>
        /// Makes the base class method testable.
        /// </summary>
        /// <param name="unitOfWork"></param>
        public new void CreateLocalRepositories(IUnitOfWork unitOfWork)
        {
            base.CreateLocalRepositories(unitOfWork);
        }

        /// <summary>
        /// Makes the base class method testable by overriding it.
        /// </summary>
        protected override IList<IApplicationFunction> LoadDefinedRaptorApplicationFunctions()
        {
            if (_definedApplicationFunctions == null)
                return base.LoadDefinedRaptorApplicationFunctions();
            return _definedApplicationFunctions;
        }

        /// <summary>
        /// Makes the base class method testable.
        /// </summary>
        /// <param name="repository">The application function repository.</param>
        /// <returns></returns>
        public new IList<IApplicationFunction> LoadApplicationFunctionsFromDatabase(IApplicationFunctionRepository repository)
        {
            return base.LoadApplicationFunctionsFromDatabase(repository);
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
        /// Makes the base class method testable.
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
        /// <param name="availableDataList">The available data list.</param>
        /// <param name="applicationFunction">The application function.</param>
        public new void AssignApplicationFunctionToRoles(IList<IAvailableData> availableDataList, IApplicationFunction applicationFunction)
        {
            base.AssignApplicationFunctionToRoles(availableDataList, applicationFunction);
        }

        /// <summary>
        /// Makes the base class method testable.
        /// </summary>
        /// <param name="applicationRoles">The application roles.</param>
        /// <param name="applicationFunction">The application function.</param>
        public new void RemoveApplicationFunctionFromRoles(IList<IApplicationRole> applicationRoles, IApplicationFunction applicationFunction)
        {
            base.RemoveApplicationFunctionFromRoles(applicationRoles, applicationFunction);
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

    }
}
