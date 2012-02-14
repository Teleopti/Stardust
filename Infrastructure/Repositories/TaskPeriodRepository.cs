using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository task period
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2007-10-31
    /// </remarks>
    public class TaskPeriodRepository : Repository<TaskPeriod>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskPeriodRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The UnitOfWork.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-10-31
        /// </remarks>
        public TaskPeriodRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
}
