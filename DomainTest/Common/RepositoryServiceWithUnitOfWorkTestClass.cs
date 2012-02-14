using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Common
{
    /// <summary>
    /// Testable Domain service.
    /// </summary>
    /// <remarks>
    /// Created by: tamasb
    /// Created date: 11/26/2007
    /// </remarks>
    public class RepositoryServiceWithUnitOfWorkTestClass : RepositoryServiceWithUnitOfWork
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryServiceWithUnitOfWorkTestClass"/> class.
        /// </summary>
        public RepositoryServiceWithUnitOfWorkTestClass()
        {
            //
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryServiceWithUnitOfWorkTestClass"/> class.
        /// </summary>
        /// <param name="repFactory">The rep factory.</param>
        /// <param name="unitOfWorkFactory">The unit of work factory.</param>
        public RepositoryServiceWithUnitOfWorkTestClass(IRepositoryFactory repFactory, IUnitOfWorkFactory unitOfWorkFactory)
            : base(repFactory, unitOfWorkFactory)
        {
            //
        }

        /// <summary>
        /// Sets the repository factory.
        /// </summary>
        /// <param name="repFactory">The rep factory.</param>
        public void SetRepositoryFactory(IRepositoryFactory repFactory)
        {
            RepositoryFactory = repFactory;
        }


        /// <summary>
        /// Sets the unit of work factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">The uow factory.</param>
        public void SetUnitOfWorkFactory(IUnitOfWorkFactory unitOfWorkFactory)
        {
            UnitOfWorkFactory = unitOfWorkFactory;
        }
    }
}
