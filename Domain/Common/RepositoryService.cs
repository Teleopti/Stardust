using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    /// <summary>
    /// Service that can create a repository
    /// </summary>
    public abstract class RepositoryService
    {
        private IRepositoryFactory _repositoryFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryService"/> class.
        /// </summary>
        /// <param name="repositoryFactory">The RepositoryFactory.</param>
        protected RepositoryService(IRepositoryFactory repositoryFactory) : this()
        {
            InParameter.NotNull("repositoryFactory", repositoryFactory);
            _repositoryFactory = repositoryFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryService"/> class.
        /// </summary>
        protected RepositoryService()
        {
            //
        }

        /// <summary>
        /// Gets or sets the repository factory interface.
        /// </summary>
        /// <value>The repository factory.</value>
        public IRepositoryFactory RepositoryFactory
        {
            get { return _repositoryFactory; }
            protected set { _repositoryFactory = value; }
        }
    }
}