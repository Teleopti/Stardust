using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Security;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// UserFunction repository class
    /// </summary>
    public class UserFunctionRepository : Repository<UserFunction>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserFunctionRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The UnitOfWork.</param>
        public UserFunctionRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            //
        }
    }
}