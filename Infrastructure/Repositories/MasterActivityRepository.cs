using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for masteractivities
    /// </summary>
    public class MasterActivityRepository : Repository<IMasterActivity>, IMasterActivityRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MasterActivityRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public MasterActivityRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}