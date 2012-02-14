using System;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Common
{
    /// <summary>
    /// Base class for domain services.
    /// </summary>
    public abstract class RepositoryServiceWithUnitOfWork : RepositoryService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryServiceWithUnitOfWork"/> class.
        /// </summary>
        protected RepositoryServiceWithUnitOfWork()
        {
            //
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryServiceWithUnitOfWork"/> class.
        /// </summary>
        /// <param name="repFactory">The RepositoryFactory.</param>
        /// <param name="unitOfWorkFactory">The UnitOfWork factory.</param>
        protected RepositoryServiceWithUnitOfWork(IRepositoryFactory repFactory, IUnitOfWorkFactory unitOfWorkFactory) :
            base(repFactory)
        {
            InParameter.NotNull("unitOfWorkFactory", unitOfWorkFactory);
            UnitOfWorkFactory = unitOfWorkFactory;
        }

        /// <summary>
        /// Gets or sets the unit of work factory interface.
        /// </summary>
        /// <value>The unit of work factory.</value>
        public IUnitOfWorkFactory UnitOfWorkFactory { get; protected set; }
    }
}