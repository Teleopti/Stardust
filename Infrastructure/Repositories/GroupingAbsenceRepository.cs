using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for BusinessUnit
    /// </summary>
    public class GroupingAbsenceRepository : Repository<GroupingAbsence>
    {
        /// <summary>
        /// Initilaze a new instance of the <see cref="GroupingAbsenceRepository"/> class 
        /// </summary>
        /// <param name="unitOfWork"></param>
        public GroupingAbsenceRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
}