using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for BusinessUnit
    /// </summary>
    public class GroupingActivityRepository : Repository<GroupingActivity>
    {
        /// <summary>
        /// Initilaze a new instance of the <see cref="GroupingActivityRepository"/> class 
        /// </summary>
        /// <param name="unitOfWork"></param>
        public GroupingActivityRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
            
        }
    }
}