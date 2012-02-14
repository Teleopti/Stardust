using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Security;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Role repository class
    /// </summary>
    public class RoleRepository : Repository<Role>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoleRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The UnitOfWork.</param>
        public RoleRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
}